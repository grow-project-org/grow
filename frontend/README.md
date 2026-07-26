# Hodowla roślin — aplikacja React

Aplikacja mobilna do zarządzania hodowlą roślin oparta o **model instancyjny**:
każda roślina to konkretny egzemplarz z własnym ID, pojemnikiem, datami i
historią zabiegów — nie statystyka. Wersja przepisana z prototypu na w pełni
typowaną aplikację React.

## Stack

- **React 18** + **TypeScript** (tryb `strict`, bez `any`)
- **Vite** — dev server i build
- **react-router-dom v6** — routing
- **@tanstack/react-query** — pobieranie danych z API (`useQuery`) i zapis (`useMutation`)
- **Formik + Yup** — formularze i walidacja
- **CSS Modules** + tokeny w `styles/theme.css` — style w plikach `.css`

## Uruchomienie

```bash
npm install
npm run dev        # http://localhost:5173
npm run build      # produkcyjny build (tsc + vite)
npm run typecheck  # sama kontrola typów
```

## Architektura

Warstwy są rozdzielone zgodnie z zasadami **SOLID** i **DRY** — logika domenowa
jest czysta (bez Reacta), stan trzyma reduktor za wąskim API, a komponenty są
„głupie” i renderują gotowe modele widoku.

```
src/
├── api/                      # WARSTWA API
│   ├── config.ts             #   bazowy URL (celowo nieistniejący) + timeout
│   ├── http.ts               #   typowany fetch: JSON, AbortController, ApiError
│   ├── dto.ts                #   format wymiany + mapowanie DTO <-> stan
│   ├── endpoints.ts          #   gardenApi.fetch / gardenApi.push
│   └── queryKeys.ts          #   klucze React Query
├── app/queryClient.ts        # QueryClient + globalna obsługa błędów -> popup
├── hooks/useGardenSync.ts    # useQuery (load+hydrate) + useMutation (push)
├── config.ts                 # stałe aplikacji (m.in. TODAY)
├── types/                    # modele domenowe (Plant, Group, LogEntry, …)
├── utils/                    # czyste helpery (daty, liczby)
├── domain/                   # LOGIKA DOMENOWA — czyste funkcje, testowalne
│   ├── species.ts            #   katalog gatunków, interwały, awatary
│   ├── schedule.ts           #   terminy zabiegów, „zaległe/dziś/za X dni”
│   ├── ids.ts                #   generowanie ID i kodów (PAP-05)
│   └── extraActions.ts       #   akcje dodatkowe (podcinanie, zbiór…)
├── data/
│   └── seed.ts               # deterministyczne dane startowe (~72 egz.)
├── state/                    # STAN — reduktor + Context API
│   ├── gardenReducer.ts      #   czysty reduktor (transakcje na stanie)
│   ├── GardenContext.tsx     #   publiczne API (useGarden) — ukrywa dispatch
│   ├── persistence.ts        #   zapis/odczyt snapshotu w localStorage
│   ├── notifications.ts      #   store powiadomień o stanie połączenia
│   └── ToastContext.tsx      #   powiadomienia (useToast)
├── routes/paths.ts           # jedno źródło prawdy o ścieżkach
├── components/
│   ├── ui/                   # prymitywy: Button, Card, Avatar, Pill, ikony…
│   ├── form/                 # TextField związany z Formik + współdzielony CSS
│   ├── sheet/                # BottomSheet + gotowe arkusze (rename, akcje)
│   ├── feedback/Toast.tsx
│   └── layout/               # PhoneFrame, BottomNav, AppShell, PageHeader
└── features/                 # EKRANY — każdy z własnym selektorem + CSS
    ├── today/                #   Dziś
    ├── plants/               #   Rośliny, Profil, Przesadzanie
    ├── add/                  #   Dodawanie
    ├── calendar/             #   Kalendarz
    └── groups/               #   Grupy
```

### API, tryb offline i synchronizacja

- **Wstępne API** (`src/api`) opisuje kontrakt snapshotowy: `GET /garden`
  ładuje cały ogród, `PUT /garden` go zapisuje. `API_BASE_URL` wskazuje
  **celowo nieistniejący host** — każde żądanie kończy się błędem, co ćwiczy
  ścieżkę offline. Podmień `VITE_API_URL`, gdy backend będzie gotowy.
- **`useQuery`** ładuje snapshot z serwera i po sukcesie hydratuje reduktor;
  odświeża się przy odzyskaniu połączenia (`refetchOnReconnect`).
- **`useMutation`** wypycha lokalne zmiany (debounce 800 ms).
- **localStorage** — każda zmiana stanu jest natychmiast zapisywana lokalnie,
  więc aplikacja działa w pełni offline; po starcie wznawia z ostatniego
  snapshotu, a po odzyskaniu połączenia synchronizuje się z backendem.
- **Obsługa błędów** — globalne handlery `QueryClient` zamieniają każdy błąd
  żądania na **pop-up** „Brak połączenia z serwerem” (z akcją „Spróbuj
  ponownie”); udane żądanie automatycznie chowa pop-up.

### Kluczowe decyzje

- **`*.selectors.ts`** — każdy ekran ma czystą funkcję budującą model widoku z
  surowego stanu. Komponenty nie liczą niczego same (SRP), co upraszcza testy i
  czytelność.
- **`useGarden()`** zwraca intencyjne metody (`commitAction`, `repot`,
  `addPlants`…) zamiast surowego `dispatch` — komponenty zależą od abstrakcji,
  nie od kształtu reduktora (Dependency Inversion).
- **Style w plikach.** Wartości tematyczne (kolory, cienie, promienie) żyją jako
  zmienne CSS w `theme.css`; komponenty używają CSS Modules. Inline pozostają
  wyłącznie wartości sterowane danymi (kolor awatara egzemplarza, kolor pigułki
  terminu).
- **Komponenty i funkcje** pisane jako `const Nazwa = () => {}`.
- **Routing** deklaratywny; ścieżki scentralizowane w `routes/paths.ts`.
- **Formularze** (dodawanie, przesadzanie, nowa grupa, zmiana etykiety) oparte o
  Formik ze schematami Yup.

### Trasy

| Ścieżka                | Ekran                     |
| ---------------------- | ------------------------- |
| `/`                    | Dziś                      |
| `/plants`              | Rośliny (dashboard)       |
| `/plants/:id`          | Profil egzemplarza        |
| `/plants/:id/repot`    | Przesadzanie              |
| `/add`                 | Nowa roślina (bulk-add)   |
| `/calendar`            | Kalendarz                 |
| `/groups`              | Grupy                     |

> Data jest przypięta do `TODAY` w `src/config.ts`, aby dane demonstracyjne
> zawsze pokazywały realistyczny mix zabiegów. Podmień na bieżącą datę do
> działania „na żywo”.
