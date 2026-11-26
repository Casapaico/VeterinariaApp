#!/usr/bin/env bash
# Pruebas de la S15: todo entra por el gateway.
#   ./scripts/probar-gateway.sh                                   -> local (http://localhost:5100)
#   ./scripts/probar-gateway.sh https://<gateway>.onrender.com    -> nube
# Opcional: VACUNAS_DIRECTO=https://<vacunas>.onrender.com para la llamada directa sin token.
GW="${1:-http://localhost:5100}"
VAC="${VACUNAS_DIRECTO:-http://localhost:5140}"
CLAVE='Vet2026*'
RESP=$(mktemp)

linea() { printf '\n\033[1m%s\033[0m\n' "$1"; }
pedir() {   # pedir "descripción" esperado método ruta [token] [json]
  local desc="$1" esp="$2" met="$3" url="$4" tok="$5" body="$6"
  local args=(-s -o "$RESP" -w '%{http_code}' -X "$met" "$url")
  [ -n "$tok" ]  && args+=(-H "Authorization: Bearer $tok")
  [ -n "$body" ] && args+=(-H 'Content-Type: application/json' -d "$body")
  local cod; cod=$(curl --max-time 90 "${args[@]}")
  local marca='OK '; [ "$cod" = "$esp" ] || marca='XX '
  printf '%s %s  %-7s %-45s %s\n' "$marca" "$cod" "$met" "${url#$GW}" "$desc"
  head -c 160 "$RESP" | tr -d '\n'; echo
}
token() {
  curl -s --max-time 90 -X POST "$GW/gateway/auth/login" -H 'Content-Type: application/json' \
    -d "{\"correo\":\"$1\",\"clave\":\"$CLAVE\"}" | python3 -c 'import sys,json; print(json.load(sys.stdin)["token"])'
}

linea "1. Auth (público en el gateway)"
pedir "login recepcionista" 200 POST "$GW/gateway/auth/login" "" "{\"correo\":\"recepcion@vetarequipa.com\",\"clave\":\"$CLAVE\"}"
pedir "login con clave incorrecta" 401 POST "$GW/gateway/auth/login" "" '{"correo":"recepcion@vetarequipa.com","clave":"mala"}'
REC=$(token recepcion@vetarequipa.com); ASI=$(token asistente@vetarequipa.com); VET=$(token veterinario@vetarequipa.com)

linea "2. GET de la API (públicos en el gateway; la API sigue exigiendo token)"
pedir "listar propietarios con token" 200 GET "$GW/gateway/propietarios" "$ASI"
pedir "mascotas filtradas por especie" 200 GET "$GW/gateway/mascotas?especie=Gato" "$ASI"
pedir "propietario inexistente" 404 GET "$GW/gateway/propietarios/999" "$ASI"
pedir "sin token: el 401 lo da la API" 401 GET "$GW/gateway/propietarios"

linea "3. Escrituras de la API (el gateway exige token; el rol lo exige la API)"
DNI=$(printf '9%07d' $((RANDOM*RANDOM % 10000000)))
pedir "crear propietario SIN token (401 del gateway)" 401 POST "$GW/gateway/propietarios" "" "{\"dni\":\"$DNI\",\"nombres\":\"Rosa\",\"apellidos\":\"Quispe Mamani\",\"telefono\":\"987111222\"}"
pedir "crear con token ALTERADO (401 del gateway)" 401 POST "$GW/gateway/propietarios" "${REC%?}x" "{\"dni\":\"$DNI\",\"nombres\":\"Rosa\",\"apellidos\":\"Quispe Mamani\",\"telefono\":\"987111222\"}"
pedir "crear con rol Asistente (403 de la API)" 403 POST "$GW/gateway/propietarios" "$ASI" "{\"dni\":\"$DNI\",\"nombres\":\"Rosa\",\"apellidos\":\"Quispe Mamani\",\"telefono\":\"987111222\"}"
pedir "crear con rol Recepcionista" 201 POST "$GW/gateway/propietarios" "$REC" "{\"dni\":\"$DNI\",\"nombres\":\"Rosa\",\"apellidos\":\"Quispe Mamani\",\"telefono\":\"987111222\"}"
ID=$(python3 -c 'import sys,json; print(json.load(open(sys.argv[1]))["propietarioId"])' "$RESP" 2>/dev/null)
pedir "actualizar el propietario creado" 204 PUT "$GW/gateway/propietarios/$ID" "$REC" '{"nombres":"Rosa María","apellidos":"Quispe Mamani","telefono":"987111333","correo":"rosa.quispe@example.com"}'
pedir "eliminar mascota con rol Recepcionista (403)" 403 DELETE "$GW/gateway/mascotas/1" "$REC"
pedir "eliminar el propietario creado" 204 DELETE "$GW/gateway/propietarios/$ID" "$REC"
pedir "eliminar propietario inexistente" 404 DELETE "$GW/gateway/propietarios/999" "$REC"

linea "4. Microservicio 2: VacunasApi (el gateway exige token)"
pedir "listar vacunas sin token (401 del gateway)" 401 GET "$GW/gateway/vacunas"
pedir "listar vacunas con token" 200 GET "$GW/gateway/vacunas" "$VET"
pedir "vacunas de gatos" 200 GET "$GW/gateway/vacunas?especie=Gato" "$ASI"
pedir "una vacuna" 200 GET "$GW/gateway/vacunas/2" "$ASI"
pedir "vacuna inexistente" 404 GET "$GW/gateway/vacunas/99" "$ASI"

linea "5. Rutas que no existen en ocelot.json"
pedir "ruta inexistente" 404 GET "$GW/gateway/citas" "$ASI"
pedir "método no declarado (POST vacunas)" 404 POST "$GW/gateway/vacunas" "$REC" '{}'

linea "6. Directo al microservicio 2, sin pasar por el gateway"
pedir "VacunasApi directo sin token" 401 GET "$VAC/api/vacunas"

rm -f "$RESP"
echo; echo "Para el 502: detén VacunasApi y ejecuta:"
echo "  curl -i $GW/gateway/vacunas -H \"Authorization: Bearer \$TOKEN\""
