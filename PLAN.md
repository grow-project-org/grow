# Plan wyrównania frontendu do backendu

> **Status: wykonany.** Fazy 0–4 zrealizowane na gałęzi
> `align-frontend-to-backend`. Odchylenia od planu i rzeczy znalezione po
> drodze opisuje [§12](#12-raport-z-wykonania). Testy frontendu pozostają
> wstrzymane zgodnie z decyzją.

Dokument roboczy. Opisuje przejście frontendu z prototypu prezentacyjnego
(stan lokalny jako źródło prawdy) na klienta backendu, oraz minimalny zestaw
zmian po stronie API, który to umożliwia.

## Spis treści

1. [Kontekst i założenia](#1-kontekst-i-założenia)
2. [Błędy typowe dla AI](#2-błędy-typowe-dla-ai)
3. [Stan faktyczny backendu](#3-stan-faktyczny-backendu)
4. [Audyt ekranów frontendu](#4-audyt-ekranów-frontendu)
5. [Backend — zmiany](#5-backend--zmiany)
6. [Backend — testy](#6-backend--testy)
7. [Frontend — co znika](#7-frontend--co-znika)
8. [Frontend — klient API](#8-frontend--klient-api)
9. [Plan fazowy](#9-plan-fazowy)
10. [Poza zakresem](#10-poza-zakresem)
11. [Czego nie zweryfikowano](#11-czego-nie-zweryfikowano)
12. [Raport z wykonania](#12-raport-z-wykonania)

---

## 1. Kontekst i założenia

Backend był budowany stopniowo i jego funkcjonalność traktujemy jako
prawidłową. Frontend powstał jako mockup na potrzeby prezentacji pomysłu —
działał na danych lokalnych (`localStorage` + reducer + seed ~72 roślin) i
opcjonalnie wypychał je do API.

**Decyzje kierunkowe:**

- Warstwa lokalna frontendu znika w całości. Traktujemy ją tak, jakby nigdy
  nie istniała; dane w niej nie mają wartości i nie podlegają migracji.
- To usuwa cały problem synchronizacji dwóch źródeł prawdy.
- Klient API zostaje wygenerowany z OpenAPI. Dotychczasowa warstwa
  `src/api/*` to prototyp i idzie do kosza.
- Szczegóły istniejące wyłącznie dla wizualizacji (emoji, rozmiar doniczki,
  prefiks kodu) są usuwane, a nie dodawane do backendu.
- Testy frontendu dodajemy dopiero po zaakceptowaniu działającej wersji.
  Testy backendu — zgodnie z istniejącą konwencją, patrz [§6](#6-backend--testy).

**Konsekwencja, którą trzeba nazwać wprost:** usunięcie stanu lokalnego
usuwa też dane, na których stoi 5 z 7 ekranów. Backend nie zwraca dziś
terminów akcji ani członkostwa w grupach. Dlatego plan zawiera dwa endpointy
odczytowe po stronie API — bez nich frontend kończy jako aplikacja CRUD-owa
na gatunki i rośliny.

---

## 2. Błędy typowe dla AI

Lista zdefiniowana przed przystąpieniem do analizy i uzupełniana przy każdej
zmianie zakresu. Służy jako checklista przy realizacji planu.

| # | Błąd | Przeciwdziałanie |
| --- | --- | --- |
| 1 | Zaufanie komentarzom zamiast kodowi. Frontend ma 14 markerów `TODO(backend)`, część nieaktualnych (`GET /api/plants` powstał po ostatniej zmianie frontu). | Kontrakt czytany z `Endpoints/*.cs` i z testów integracyjnych. |
| 2 | Zaufanie README. `frontend/README.md` opisuje `GET /garden`, `PUT /garden` i hook `useGardenSync` — nie istnieją od commita `5314a96`. | README traktowany jako artefakt do naprawy, nie jako opis stanu. |
| 3 | Wymyślanie endpointów, których nie ma. | Każdy endpoint w §3 ma numer linii w pliku. |
| 4 | Mylenie „backend ma handler" z „backend ma endpoint". `GetPlantActionScheduleCommandHandler` istnieje i jest otestowany, ale nie jest zmapowany na HTTP. | Rozdzielone jawnie w §3. |
| 5 | Przeskakiwanie w naprawianie backendu zamiast wyrównywania frontu. | Zmiany backendu ograniczone do §5 i uzasadnione odblokowaniem konkretnych ekranów. |
| 6 | Raportowanie różnicy tam, gdzie jej nie ma. Serializacja `Dictionary<PlantActionType, TimeSpan>` była w prototypie obsłużona poprawnie. | Zweryfikowane, nie wpisane jako luka. |
| 7 | Rozrost zakresu pod pozorem porządkowania. | Warstwa prezentacji frontendu zostaje nietknięta. |
| 8 | Płaska lista bez priorytetów. | Podział na to, co blokuje, co jest niewykorzystane i co jest do usunięcia. |
| 9 | Pominięcie faktu, że frontend ma seed 72 roślin wypychany do API po zalogowaniu. | Seed usuwany w Fazie 1. |
| 10 | Udawanie, że aplikacja została uruchomiona. | Patrz §11. |
| 11 | Usunięcie warstwy lokalnej połowicznie — zostawienie `localStorage` „jako cache" albo reducera „na razie". | §7 to lista plików do skasowania, nie do refaktoru. React Query jest jedynym cache'em. |
| 12 | Udawanie, że ekran działa — np. `lastWater = null` dla wszystkich, przez co widok „Dziś" renderuje się i kłamie. | Ekran bez danych jest wyłączany z routingu, nie zasilany placeholderem. |
| 13 | Ciche zachowanie pól bez pokrycia przez hardkodowanie wartości domyślnych. | Pola idą do usunięcia. Wyjątek: wartości wyliczane z realnych danych (kolor awatara z Guida, ikona grupy z `GroupType`). |
| 14 | Traktowanie „przepisujemy" jako licencji na nowy design system. | `components/ui`, `components/layout`, `styles/`, routing — bez zmian. |
| 15 | Traktowanie wygenerowanego klienta jako gotowego produktu. W backendzie jest zero wystąpień `WithName` i `Produces`; `UpdateInterval` zwraca `IResult`. | Ograniczenia i minimalna poprawka metadanych opisane w §8. |
| 16 | Dodanie testów frontendu wbrew decyzji. | Faza testowa frontendu wypada; warunek powrotu w §10. |
| 17 | Przeoczenie, że aplikacja przestaje działać bez logowania. `AccountSheet` mówi dziś wprost „Aplikacja działa również bez logowania". | Route guard i zmiana treści to osobna pozycja Fazy 3. |
| 18 | Przeoczenie zmiany typu identyfikatora. Lokalnie `Plant.id` to `number` (`avatarBg` indeksuje tablicę modulo), na serwerze Guid. | Osobna pozycja Fazy 1. |
| 19 | Dopisywanie komentarzy wyjaśniających. Backend ma zero komentarzy poza `//todo` — to konwencja, nie przeoczenie. | Kod z §5 nie ma komentarzy. `//todo` dozwolone jako znacznik intencji. Ta sama reguła obowiązuje nowy kod frontendu. |
| 20 | Zgadywanie konwencji zamiast sprawdzenia. `.editorconfig` wymusza `this.` dla pól, właściwości i metod na poziomie **error**; obowiązuje `var`, `_ =` na odrzucanych zwrotkach, rekordy query/result w jednym pliku z handlerem, DTO ze statyczną fabryką `From`. | Konwencje sprawdzone w `.editorconfig` i w istniejących handlerach. Handlery rejestruje Scrutor (`CqrsModule`) — nie trzeba nic dopisywać do DI. |

---

## 3. Stan faktyczny backendu

### Wystawione endpointy

| Metoda | Ścieżka | Wejście | Wyjście | Plik |
| --- | --- | --- | --- | --- |
| POST | `/api/users/register` | `{email, username}` | 200, puste | `UsersEndpoints.cs:28` |
| POST | `/api/users/login` | `{email, password}` (hasło ignorowane) | 200 + cookie `__Host-Auth` | `UsersEndpoints.cs:29` |
| POST | `/api/users/me` | — | `{username}` | `UsersEndpoints.cs:30` |
| GET | `/api/users/csrf` | — | token (`text/plain`) | `UsersEndpoints.cs:32` |
| GET | `/api/species` | `from`, `limit` (1–100, wymagane), `searchName?` | `SpecieDto[]` | `SpeciesEndpoints.cs:27` |
| POST | `/api/species` | `{name}` | `{specieId}` | `SpeciesEndpoints.cs:21` |
| POST | `/api/species/{id}/interval/{Watering\|Fertilizing}` | `{interval: TimeSpan}` | 200 / 404 | `SpeciesEndpoints.cs:22` |
| GET | `/api/plants` | `searchText?`, `from`, `limit` (1–100) | `PlantDto[]` | `PlantsEndpoints.cs:31` |
| POST | `/api/plants` | `{customId, specieId}` | `{createdPlantId}` | `PlantsEndpoints.cs:27` |
| POST | `/api/plants/{id}/events` | `{type: 0\|1, executedAt}` | `{plantEventId}` | `PlantsEndpoints.cs:28` |
| POST | `/api/plants/{id}/groups/{groupId}` | — | 200 | `PlantsEndpoints.cs:29` |
| DELETE | `/api/plants/{id}/groups/{groupId}` | — | 200 | `PlantsEndpoints.cs:30` |
| POST | `/api/plant-groups` | `{name, type: 1\|2\|3}` | `{createdPlantGroupId}` | `PlantsEndpoints.cs:35` |

### Istnieje w domenie, nie jest wystawione

- `GetPlantActionScheduleCommand` / `...Handler` — liczy `NextDate` dla pary
  (roślina, typ akcji), otestowany, niezmapowany na HTTP.
- `UsersEndpoints.Logout` — metoda istnieje, `MapUsersEndpoints` jej nie
  rejestruje.

### Nie istnieje

Odczyt grup, odczyt historii rośliny, usunięcie lub cofnięcie zdarzenia,
edycja i usuwanie roślin/gatunków/grup, typy zdarzeń inne niż `Watering`
i `Fertilizing`, rozmiar doniczki.

### Szczegóły kontraktu, których nie wolno „poprawiać" w kliencie

- Enum w ciele żądania jest liczbą (`Watering = 0`, `Fertilizing = 1`),
  ale w ścieżce `/interval/{actionType}` jest nazwą.
- `GroupType` nie zaczyna się od zera: `Region = 1`, `WorkGroup = 2`,
  `TemporaryGroup = 3`.
- Klucze słownika `Intervals` serializują się jako nazwy enuma w PascalCase,
  w odróżnieniu od camelCase pozostałych pól.
- `TimeSpan` jako `"7.00:00:00"`.
- `GET`-y nie zwracają total count — paginacja polega na ciągnięciu do
  momentu, gdy strona jest krótsza niż `limit`.

---

## 4. Audyt ekranów frontendu

Każdy selektor prototypu czyta pola, których backend nie zwraca.

| Ekran | Czego potrzebuje | Backend to zwraca |
| --- | --- | --- |
| Dziś | ostatnie podlanie/nawożenie + interwał gatunku | **nie** |
| Kalendarz | jw. + etykieta regionu z grup | **nie** |
| Rośliny (dashboard) | jw., liczniki „do podlania" | **nie** |
| Profil rośliny | historia zdarzeń, doniczka, grupy | **nie** |
| Grupy | lista grup + członkostwo | **nie** |
| Dodawanie rośliny | gatunki + `POST /api/plants` | tak |
| Konto | login / register / me | tak (bez wylogowania) |

Po zmianach z §5 kolumna trzecia zmienia się na „tak" dla wierszy 1–3 i 5.
Profil rośliny pozostaje częściowy (ostatnie wykonanie każdego typu akcji,
bez pełnej historii).

---

## 5. Backend — zmiany

Dwa endpointy odczytowe. Bez migracji — same odczyty istniejących tabel.
CORS już przepuszcza `GET`. Handlery rejestruje Scrutor automatycznie.

### 5.1 `PlantDto` + rozszerzony `SearchPlantsQuery`

Odblokowuje: Dziś, Kalendarz, dashboard Roślin, etykiety lokalizacji.
Wykorzystuje istniejące `PlantActionSchedule.CalculateNextDate`, zamiast
powielać logikę terminu w TypeScripcie.

`Grow.WebApi/Dtos/PlantDto.cs` — zastępuje obecny:

```csharp
using Grow.Domain.Commons;
using Grow.Domain.Plants;
using Grow.Domain.Species;

namespace Grow.WebApi.Dtos;

public record PlantDto(
    Guid Id,
    string CustomId,
    Guid SpecieId,
    IReadOnlyCollection<Guid> PlantGroupIds,
    IReadOnlyDictionary<PlantActionType, DateTime> LastExecutions,
    IReadOnlyDictionary<PlantActionType, DateOnly> NextDates)
{
    public static PlantDto From(Plant plant, Specie specie)
    {
        var lastExecutions = plant.Events
            .GroupBy(x => x.Type)
            .ToDictionary(x => x.Key, x => x.Max(e => e.ExecutedAt));

        var nextDates = lastExecutions
            .Where(x => specie.Intervals.ContainsKey(x.Key))
            .ToDictionary(x => x.Key, x => PlantActionSchedule.CalculateNextDate(x.Value, specie.Intervals[x.Key]));

        return new(
            plant.Id,
            plant.CustomId,
            plant.SpecieId,
            [.. plant.PlantGroupMemberships.Select(x => x.PlantGroupId)],
            lastExecutions,
            nextDates);
    }
}
```

`Grow.Domain/Plants/Handlers/SearchPlantsQueryHandler.cs` — zmiana punktowa.
Istniejące wyrażenie wyszukiwania zostaje nietknięte, mimo że
`query.SearchText == null` wewnątrz `if`a jest zbędne — to nie jest miejsce
na tę poprawkę.

```csharp
public record SearchPlantsQueryResult(IEnumerable<Plant> Plants, IReadOnlyDictionary<Guid, Specie> Species);
```

```csharp
        var user = userSessionProvider.Get();
        var plantsQuery = databaseContext.Plants
            .Include(x => x.Events)
            .Include(x => x.PlantGroupMemberships)
            .Where(x => x.OwnerId == user.Id);

        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            var specieIds = await databaseContext.Species
                .Where(x => x.Name.Contains(query.SearchText))
                .Select(x => x.Id)
                .ToArrayAsync(ct);

            plantsQuery = plantsQuery.Where(x => query.SearchText == null ||
                x.CustomId.Contains(query.SearchText) ||
                specieIds.Contains(x.SpecieId));
        }

        plantsQuery = plantsQuery.Skip(query.From).Take(query.Limit);

        var plants = await plantsQuery.ToArrayAsync(ct);

        var specieIdsOfPlants = plants.Select(x => x.SpecieId).Distinct().ToArray();
        var species = await databaseContext.Species
            .Where(x => specieIdsOfPlants.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        return new SearchPlantsQueryResult(plants, species);
```

`Grow.WebApi/Endpoints/PlantsEndpoints.cs` — jedna linia w `SearchPlants`:

```csharp
        return plants.Plants.Select(x => PlantDto.From(x, plants.Species[x.SpecieId]));
```

### 5.2 `GET /api/plant-groups`

Odblokowuje: ekran Grup, regiony, akcje zbiorcze.

`Grow.Domain/Plants/Handlers/GetPlantGroupsQueryHandler.cs` — nowy plik:

```csharp
using Grow.Domain.Commons;
using Grow.Infrastructure.Cqrs;
using Microsoft.EntityFrameworkCore;

namespace Grow.Domain.Plants.Handlers;

public record GetPlantGroupsQuery(int From, int Limit, string? SearchName) : IQuery<GetPlantGroupsQueryResult>;
public record GetPlantGroupsQueryResult(IEnumerable<PlantGroup> PlantGroups);

public class GetPlantGroupsQueryHandler(IDatabaseContext databaseContext, IAuthUserSessionProvider userSessionProvider) : IQueryHandler<GetPlantGroupsQuery, GetPlantGroupsQueryResult>
{
    public async Task<GetPlantGroupsQueryResult> HandleAsync(GetPlantGroupsQuery query, CancellationToken ct)
    {
        var user = userSessionProvider.Get();
        var plantGroups = await databaseContext.PlantGroups
            .Include(x => x.PlantGroupMemberships)
            .Where(x => x.OwnerId == user.Id)
            .Where(x => query.SearchName == null || x.Name.Contains(query.SearchName))
            .Skip(query.From)
            .Take(query.Limit)
            .ToArrayAsync(ct);

        return new GetPlantGroupsQueryResult(plantGroups);
    }
}
```

`Grow.WebApi/Dtos/PlantGroupDto.cs` — nowy plik:

```csharp
using Grow.Domain.Plants;

namespace Grow.WebApi.Dtos;

public record PlantGroupDto(Guid Id, string Name, GroupType Type, IReadOnlyCollection<Guid> PlantIds)
{
    public static PlantGroupDto From(PlantGroup plantGroup)
        => new(
            plantGroup.Id,
            plantGroup.Name,
            plantGroup.Type,
            [.. plantGroup.PlantGroupMemberships.Select(x => x.PlantId)]);
}
```

`Grow.WebApi/Endpoints/PlantsEndpoints.cs`:

```csharp
        _ = plantGroup.MapGet("/", GetPlantGroups);
```

```csharp
    public static async Task<IEnumerable<PlantGroupDto>> GetPlantGroups(
        IDispatcher dispatcher, [FromQuery] int from, [FromQuery] [Range(1, 100)] int limit, [FromQuery] string? searchName = null, CancellationToken ct = default)
    {
        var plantGroups = await dispatcher.QueryAsync<GetPlantGroupsQuery, GetPlantGroupsQueryResult>(new GetPlantGroupsQuery(from, limit, searchName), ct);
        return plantGroups.PlantGroups.Select(PlantGroupDto.From);
    }
```

### 5.3 Uwagi

- **Redundancja jest zamierzona.** `PlantDto.PlantGroupIds` i
  `PlantGroupDto.PlantIds` to ta sama relacja z dwóch stron. Bez pierwszego
  lista roślin nie pokaże lokalizacji bez pobrania wszystkich grup; bez
  drugiego karta grupy nie zna składu. To model odczytowy — jeden join
  w każdą stronę.
- **`Include` pojawia się tu pierwszy raz w projekcie.** `Include` razem ze
  `Skip`/`Take` na kolekcjach EF opakuje w podzapytanie. Działa, ale warto
  zerknąć na wygenerowany SQL przy większych ogrodach.
- **Bez zmian w DI i bez migracji.**

---

## 6. Backend — testy

Do wszystkiego, co dokładamy w §5, dopisujemy testy — mimo że testy
frontendu są wstrzymane. Ta decyzja dotyczyła frontendu, którego kod może
się jeszcze mocno zmienić. Backend ma pełne pokrycie w `Grow.Tests.Unit`
i `Grow.Tests.Integration`; wprowadzanie do niego nieotestowanych endpointów
zepsułoby istniejący standard i wynik w codecov.

Testy piszemy zgodnie z konwencją, która już w projekcie obowiązuje —
bez wprowadzania nowych wzorców, bibliotek ani stylu nazewnictwa.

**Testy jednostkowe** (`Grow.Tests.Unit/Domain/Plants/Handlers/`):

- NUnit, `[TestFixture]`, klasa `<NazwaHandlera>Tests`.
- Moq + `MockQueryable.Moq` — prywatne fabryki `CreateContextMock(...)`
  i `CreateUserSessionProviderMock(Guid userId)` jak w
  `SearchPlantsQueryHandlerTests`.
- Nazewnictwo: `HandleAsync_WhenSearchTextMatchesCustomId_ReturnsMatchingPlants`.
- `Assert.That(...)`, `Assert.EnterMultipleScope()` przy wielu asercjach.
- Encje budowane przez fabryki domenowe (`Plant.Create`, `Specie.Create`,
  `PlantGroup.CreateRegion`), nigdy przez konstruktory.

**Testy integracyjne** (`Grow.Tests.Integration/Endpoints/`):

- Dziedziczenie po `IntegrationTestBase`; helpery `CreateSpecieAsync`,
  `CreatePlantAsync`, `CreateOtherUserClientAsync`,
  `factory.CreateDbContextAsync()`.
- Nazewnictwo: `GetPlantGroups_WhenSearchNameIsProvided_ShouldReturnOnlyMatchingGroups`
  (integracyjne używają `Should...`, jednostkowe formy oznajmującej —
  utrzymujemy ten podział).
- Zakres minimalny, wzorowany na `SpeciesTests`:
  - pusta lista, gdy nic nie istnieje,
  - zwracanie utworzonych rekordów,
  - `limit` mniejszy niż liczba rekordów → skrócona lista,
  - `searchName` / `searchText` → tylko pasujące,
  - `limit=0` i `limit=101` → `BadRequest`,
  - **izolacja właścicieli** — dane innego użytkownika nie wyciekają
    (ten test jest w projekcie przy każdym handlerze i tu też musi być),
  - dla `PlantDto`: `NextDates` puste, gdy gatunek nie ma interwału;
    `NextDates` wyliczone, gdy jest interwał i istnieje zdarzenie;
    `LastExecutions` bierze najpóźniejsze zdarzenie danego typu;
    `PlantGroupIds` odzwierciedla członkostwo po `AddToGroup`/`RemoveFromGroup`.

Bez komentarzy w kodzie testów — nazwa testu jest opisem.

---

## 7. Frontend — co znika

### Pola modelu

`emoji` (roślina, gatunek, grupa), `potL`, `potCm`, `prefix`, `remoteId`,
`syncedToServer`, `done` / `DoneMap`, `dismissed`, `LogEntry`, `LogType`
poza `water` i `fert`.

### Pliki do skasowania

```
src/data/seed.ts
src/state/persistence.ts
src/state/gardenReducer.ts
src/state/GardenContext.tsx
src/hooks/useBackendSync.ts
src/config.ts
src/domain/extraActions.ts
src/api/dto.ts
src/api/endpoints.ts
src/api/http.ts
src/api/config.ts
src/features/plants/RepotPage.tsx
src/features/plants/RepotPage.module.css
src/components/sheet/HarvestSheet.tsx
src/components/sheet/CustomEventSheet.tsx
src/components/sheet/ActionGridSheet.tsx
src/components/sheet/ActionGridSheet.module.css
```

### Funkcje UI

Przesadzanie, rozmiar doniczki, podcinanie, zbiór, własne zdarzenie,
wyrównanie harmonogramu grupy, cofanie odhaczenia, bulk-add (N requestów bez
transakcji — wraca dopiero, gdy backend dostanie endpoint zbiorczy).

### Co zostaje, bo jest wyliczane z realnych danych

- Kolor awatara — hash z Guida rośliny zamiast `id % 6`.
- Ikona grupy — z `GroupType`, który backend zwraca.
- `relLabel` — etykiety „wczoraj", „za 3 dni".

### Co zostaje nietknięte

`components/ui`, `components/form`, `components/layout`,
`components/feedback`, `styles/`, `routes/paths.ts`, `ToastContext`,
`notifications`, `queryClient`.

### Zmiany wymuszone przez model serwera

- `Plant.id`: `number` → `string` (Guid). Trasy `/plants/:id` przyjmują Guid.
- Kod rośliny (`customId`) jest naturalnym kluczem unikalnym per właściciel —
  używany w UI, nie do routingu.
- Aplikacja wymaga logowania. Treść w `AccountSheet` („Aplikacja działa
  również bez logowania") jest nieprawdziwa i musi zniknąć.

---

## 8. Frontend — klient API

1. **Emisja `openapi.json` przy buildzie.** `MapOpenApi()` jest wywoływane
   tylko pod `IsDevelopment()`, więc generowanie z żywego serwera wymagałoby
   odpalania API w CI. Zamiast tego `Microsoft.Extensions.ApiDescription.Server`
   emituje dokument w trakcie `dotnet build`.
2. **Minimalne metadane w backendzie** — zmiana nie-funkcjonalna, ale
   konieczna dla sensownego klienta:
   - `.WithName("GetSpecies")` itd. na każdym mapowaniu → `operationId` →
     czytelne nazwy metod. Dziś w projekcie jest zero wystąpień `WithName`,
     więc generator nazwałby metody od ścieżek.
   - `UpdateInterval`: `IResult` → `Results<Ok, NotFound>`, inaczej brak
     schematu odpowiedzi.
   - `GET /api/users/csrf` zwraca goły `string` (`text/plain`) — opakować
     w obiekt albo oznaczyć jawnie.
3. **Generator:** `openapi-typescript` (typy) + `openapi-fetch` (klient).
   Frontend stoi na `fetch` + React Query; `openapi-fetch` waży ~2 kB, nie
   narzuca własnego stanu i pozwala ustawić `credentials: 'include'`
   w jednym miejscu, czyli zastępuje `api/http.ts` jeden do jednego.
   Kiota, NSwag i orval dokładają warstwę, której tu nie potrzebujemy.
4. **Co zostaje ręczne:** interceptor 401 → wylogowanie i powrót do bramki,
   nagłówek `X-CSRF-TOKEN` z `GET /api/users/csrf`, mapowanie błędu na popup
   w `queryClient.ts`.
5. **Czego nie „poprawiamy" w kliencie:** asymetrie kontraktu wymienione
   na końcu §3. To jest kontrakt, nie błąd.

---

## 9. Plan fazowy

| Faza | Zakres | Zależność |
| --- | --- | --- |
| **0** | Backend: §5.1 + §5.2 + testy wg §6 | — |
| **1** | Usunięcie warstwy lokalnej (§7). React Query jedynym cache'em. `Plant.id: number → string`, trasy na Guid | — |
| **2** | `openapi.json` z buildu, metadane endpointów, wygenerowany klient (§8) | 0 |
| **3** | Logowanie jako bramka (route guard, zmiana treści w `AccountSheet`). Gatunki: lista, wyszukiwanie, interwały. Rośliny: lista, wyszukiwanie, dodawanie | 2 |
| **4** | Dziś, Kalendarz, dashboard Roślin, Grupy, regiony, akcje zbiorcze — na `NextDates` i `PlantGroupIds` z Fazy 0 | 0, 3 |

Faza 0 idzie równolegle z 1 i 2 — nie blokują się.

Po Fazie 3 aplikacja jest używalna w minimalnym zakresie (logowanie,
gatunki, rośliny, odhaczanie akcji bez podglądu terminu). Po Fazie 4
odzyskuje pełny zakres prototypu poza funkcjami z §10.

---

## 10. Poza zakresem

### Backlog backendu, świadomie nietykany

- `GET /api/plants/{id}/events` — pełna historia uprawy. `PlantDto.LastExecutions`
  daje tylko ostatnie wykonanie każdego typu.
- `DELETE /api/plants/{id}/events/{eventId}` — cofanie odhaczenia.
- Rejestracja `/api/users/logout` — metoda `UsersEndpoints.Logout` istnieje,
  `MapUsersEndpoints` jej nie dodaje. Jedna linia, jeśli ma trafić do Fazy 0.
- Statusy błędów domenowych: `ArgumentException` (duplikat `customId`,
  duplikat nazwy grupy) daje 500 zamiast 400/409, brak `UseExceptionHandler`.
  Frontend nie odróżni „kod zajęty" od „serwer padł".
- Brak unikalności `Users.Email` — rejestracja tego samego adresu tworzy
  drugie konto.
- Walidacja CSRF dla JSON. `UseAntiforgery()` sprawdza dziś tylko form-data,
  więc ochrona działa przez przypadek, nie przez projekt.
- Endpoint zbiorczego tworzenia roślin (warunek powrotu bulk-add).

### Testy frontendu

Wstrzymane do zaakceptowania działającej wersji. Warunek powrotu: zacząć od
parsowania `TimeSpan` i mapowania enumów — to jedyne miejsca, których
wygenerowany klient nie osłania typami.

### Czego plan nie robi

- Nie zachowuje żadnych danych z wersji lokalnej.
- Nie przepisuje warstwy prezentacji frontendu.
- Nie zmienia logiki domenowej backendu.

---

## 11. Czego nie zweryfikowano

- Żaden z projektów nie został uruchomiony. Analiza opiera się na kodzie
  i na testach integracyjnych jako źródle prawdy o kontrakcie HTTP.
- Fakt, że `GET /api/species` bez `from` i `limit` zwraca 400, wynika z reguł
  bindingu minimal API oraz z tego, że wszystkie testy podają te parametry —
  nie z zaobserwowanej odpowiedzi. Po Fazie 1 traci znaczenie, bo ta warstwa
  idzie do kosza.
- Wygenerowany SQL dla `Include` + `Skip`/`Take` (§5.3) nie był sprawdzony.

---

## 12. Raport z wykonania

### Co powstało

**Backend** — `PlantDto` rozszerzony o `plantGroupIds`, `lastExecutions`
i `nextDates`; `SearchPlantsQuery` dociąga gatunki i nawigacje; nowy
`GET /api/plant-groups` z `GetPlantGroupsQueryHandler` i `PlantGroupDto`.
Wszystko bez migracji i bez zmian w DI.

**Kontrakt** — `contracts/openapi.json` generowany przy `dotnet build`
(`Microsoft.Extensions.ApiDescription.Server`). Każdy endpoint ma `WithName`,
`UpdateInterval` zwraca `Results<Ok, NotFound>`, `/api/users/csrf` zwraca
`CsrfTokenResponse` zamiast gołego stringa.

**Frontend** — warstwa lokalna usunięta w całości (17 plików), klient
generowany (`openapi-typescript` + `openapi-fetch`), `GardenContext` przepisany
na React Query, logowanie jako bramka, wszystkie sześć ekranów działa na danych
z serwera.

### Odchylenia od planu

| Co | Dlaczego |
| --- | --- |
| **Naprawiono `AddPlantToGroupCommandHandler` i `RemovePlantFromGroupCommandHandler`** — oba ładowały grupę bez `Include(x => x.PlantGroupMemberships)`. Usuwanie z grupy było więc ciche no-op, a zabezpieczenie przed duplikatem w `AddPlant` martwe. | Błąd wyszedł dopiero na nowych testach integracyjnych. Bez tej poprawki oba dodane endpointy odczytowe zwracałyby nieaktualne członkostwo, czyli nie odblokowałyby ekranu Grup. |
| **Usunięto zdublowane `AddValidation()`** w `Program.cs`. | Powodowało `CS8785` — `ValidationsGenerator` nie generował źródeł. Build ma teraz 0 ostrzeżeń, a dokument OpenAPI generuje się poprawnie. |
| **Naprawiono `tsconfig.node.json`** (brak `@types/node`, `noEmit` w projekcie composite). | `tsc -b` nie przechodził jeszcze przed tą zmianą, więc nie dało się zweryfikować przepisania. |
| **Kalendarz dostał nawigację po miesiącach.** | Był przypięty na sztywno do lipca 2026 razem z `TODAY`. Po odpięciu daty musiał umieć pokazać dowolny miesiąc. |
| **Widok „Dziś" pokazuje też pozycje odhaczone dzisiaj.** | W prototypie odhaczenie wypychało roślinę z listy (termin przeskakiwał w przód), więc `CheckToggle` i styl `doneText` były martwe. Teraz filtr to `isDue || isDoneToday`, a odhaczone są wyszarzone i zablokowane — cofnięcia nie ma. |
| **`Avatar` przyjmuje `label` zamiast `emoji`** i pokazuje pierwszą literę nazwy gatunku. | Emoji było polem bez pokrycia w API. Kolor tła liczony z hasha Guida rośliny. |
| **Ostrzeżenie o mieszanych harmonogramach straciło przycisk „zignoruj".** | `dismissed` było stanem wyłącznie lokalnym. |
| **Rejestracja przy logowaniu odpala się też na 5xx**, nie tylko 4xx. | Backend na nieistniejący e-mail rzuca `NullReferenceException` → 500. Gdyby warunek wymagał 4xx, założenie konta nie zadziałałoby nigdy. Blokada dotyczy statusu 0 (timeout, brak sieci) i to ona chroni przed duplikatami. |

### Weryfikacja

- **Backend:** 162 testy przechodzą (103 jednostkowe, 59 integracyjnych;
  przed zmianą 137). Nowe: 7 jednostkowych dla `GetPlantGroupsQueryHandler`
  i `SearchPlantsQuery`, 18 integracyjnych dla `PlantDto` i `/api/plant-groups`
  wraz z testami izolacji właścicieli.
- **Kontrakt:** serializacja sprawdzona empirycznie na działającym API, nie
  z komentarzy prototypu. Klucze słowników to nazwy enuma (`Watering`,
  `Fertilizing`), `nextDates` to czyste daty (`2026-09-23`), `lastExecutions`
  to znaczniki UTC, `intervals` to `7.00:00:00`.
- **End-to-end na realnym Postgresie:** rejestracja → logowanie → `me` → CSRF →
  utworzenie gatunku → interwał → utworzenie rośliny → grupa → przypisanie →
  zdarzenie → odczyt `/api/plants` i `/api/plant-groups`. Osobno potwierdzono
  poprawkę członkostwa: podwójne dodanie nie duplikuje, `DELETE` faktycznie
  usuwa.
- **Frontend:** `tsc -b` bez błędów, `vite build` przechodzi, dev server
  serwuje i transformuje moduły, brak odwołań do usuniętych modułów.
- **Czego nie zweryfikowano:** klikalnej ścieżki przez UI w przeglądarce —
  w tej sesji nie było narzędzi przeglądarkowych.

### Pozostaje otwarte

Backlog backendu z [§10](#10-poza-zakresem) jest nieruszony: historia uprawy,
cofanie zdarzenia, `/api/users/logout`, kody statusów dla błędów domenowych,
unikalność `Users.Email`, walidacja CSRF dla JSON, tworzenie roślin hurtem.
