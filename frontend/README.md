# Grow — frontend

Aplikacja mobilna do zarządzania hodowlą roślin. Klient `Grow.WebApi` — nie ma
stanu lokalnego ani trybu offline: serwer jest jedynym źródłem prawdy, a React
Query jedynym cache'em.

## Stack

- **React 18** + **TypeScript** (`strict`, bez `any`)
- **Vite** — dev server i build
- **react-router-dom v6** — routing
- **@tanstack/react-query** — pobieranie i zapis danych
- **openapi-typescript** + **openapi-fetch** — klient API generowany z kontraktu
- **Formik + Yup** — formularze i walidacja
- **CSS Modules** + tokeny w `styles/theme.css`

## Uruchomienie

```bash
yarn install
yarn dev            # http://localhost:5173
yarn build          # tsc -b + vite build
yarn api:generate   # regeneracja typów z ../contracts/openapi.json
```

Backend musi działać pod adresem z `VITE_API_URL` (domyślnie
`https://localhost:7122`). CORS po stronie API dopuszcza `http://localhost:5173`.

## Kontrakt API

`contracts/openapi.json` powstaje przy `dotnet build` projektu `Grow.WebApi`
(`Microsoft.Extensions.ApiDescription.Server`). Po zmianie endpointów:

```bash
cd backend && dotnet build Grow.WebApi
cd ../frontend && yarn api:generate
```

`src/api/schema.d.ts` jest generowany — nie edytuj go ręcznie.

## Architektura

```text
src/
├── api/
│   ├── schema.d.ts       # GENEROWANE z OpenAPI
│   ├── client.ts         # openapi-fetch + cookie, CSRF, timeout, 401, ApiError
│   ├── resources.ts      # typowane wywołania + stronicowanie
│   └── queryKeys.ts
├── app/queryClient.ts    # QueryClient + globalna obsługa błędów -> popup
├── types/                # modele widoku (Plant, Species, Group)
├── domain/               # czyste funkcje: mapowanie DTO, harmonogram, kody
├── state/
│   ├── AuthContext.tsx   # sesja, bramka logowania, reakcja na 401
│   ├── GardenContext.tsx # dane z API + mutacje (useGarden)
│   └── ToastContext.tsx
├── routes/paths.ts
├── components/           # ui, form, sheet, feedback, layout
└── features/             # auth, today, plants, add, calendar, groups
```

### Dane i synchronizacja

- Każdy ekran czyta z `useGarden()`, które opakowuje trzy zapytania:
  `/api/species`, `/api/plants`, `/api/plant-groups`. Wszystkie są stronicowane
  po 100 rekordów aż do wyczerpania — API nie zwraca licznika całości.
- Mutacja zawsze kończy się unieważnieniem odpowiednich kluczy zapytań.
- Terminy (`nextDates`) liczy backend; frontend ich nie przelicza.
- Stan „odhaczone dziś" wynika z `lastExecutions` — nie ma lokalnej flagi.
- 401 z dowolnego żądania przestawia sesję na anonimową i czyści cache.

### Logowanie

Aplikacja wymaga zalogowania. Backend nie weryfikuje jeszcze hasła
(`SessionStorage` sprawdza tylko `IsUserVerified`), więc formularz zbiera e-mail
i nazwę użytkownika. Logowanie nieistniejącym adresem kończy się po stronie API
błędem 500, który traktujemy jako „konto nie istnieje" i zakładamy je
automatycznie. Brak odpowiedzi serwera (status 0) **nie** powoduje rejestracji —
inaczej awaria sieci tworzyłaby duplikaty kont.

### Czego nie ma

Poniższe nie mają odpowiednika w API i zostały usunięte, a nie zaślepione:
przesadzanie i rozmiar doniczki, podcinanie, zbiór, zdarzenia własne, pełna
historia uprawy, cofanie odhaczenia, wyrównanie harmonogramu grupy, wylogowanie
oraz dodawanie wielu roślin naraz.

### Trasy

| Ścieżka       | Ekran                  |
| ------------- | ---------------------- |
| `/`           | Dziś                   |
| `/plants`     | Rośliny                |
| `/plants/:id` | Profil egzemplarza     |
| `/add`        | Nowa roślina           |
| `/calendar`   | Kalendarz              |
| `/groups`     | Grupy                  |
