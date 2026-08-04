# Grow.Infrastructure.Database

Zawiera `DatabaseContext` (EF Core + Npgsql) oraz migracje bazy danych.

## Konfiguracja

Connection string jest wstrzykiwany przez DI (`AddDbContextFactory<DatabaseContext>` w
`Grow.WebApi/Program.cs`) i pobierany z konfiguracji `ConnectionStrings:Postgres`
(patrz `Grow.WebApi/appsettings.Development.json`).

Do tworzenia migracji (poza uruchomionym hostem WebApi) służy
`Database/DatabaseContextFactory.cs` (`IDesignTimeDbContextFactory<DatabaseContext>`).
Odczytuje ten sam connection string ze zmiennej `ConnectionStrings__Postgres`,
a jeśli jej nie ma — używa domyślnego `Host=localhost;Username=postgres;Database=postgres`.

## Wymagania

Narzędzie `dotnet-ef` (raz na maszynę):

```bash
dotnet tool install --global dotnet-ef
```

## Tworzenie migracji

Uruchamiaj z katalogu `backend`. Zarówno `--project`, jak i `--startup-project`
muszą wskazywać na `Grow.Infrastructure.Database` (dzięki `DatabaseContextFactory` narzędzie
nie potrzebuje uruchamiać hosta `Grow.WebApi`):

```bash
dotnet ef migrations add <NazwaMigracji> --project Grow.Infrastructure.Database --startup-project Grow.Infrastructure.Database --output-dir Migrations
```

Przykład:

```bash
dotnet ef migrations add InitialCreate --project Grow.Infrastructure.Database --startup-project Grow.Infrastructure.Database --output-dir Migrations
```

## Odpalanie migracji (update bazy)

```bash
dotnet ef database update --project Grow.Infrastructure.Database --startup-project Grow.Infrastructure.Database
```

Migracje można też stosować automatycznie przy starcie aplikacji, wywołując
`dbContext.Database.MigrateAsync()` w `Grow.WebApi/Program.cs`.

## Inne przydatne komendy

Cofnięcie ostatniej niezaaplikowanej migracji (usuwa plik migracji):

```bash
dotnet ef migrations remove --project Grow.Infrastructure.Database --startup-project Grow.Infrastructure.Database
```

Lista migracji:

```bash
dotnet ef migrations list --project Grow.Infrastructure.Database --startup-project Grow.Infrastructure.Database
```

Cofnięcie bazy do konkretnej migracji (lub `0`, żeby cofnąć wszystkie):

```bash
dotnet ef database update <NazwaMigracji> --project Grow.Infrastructure.Database --startup-project Grow.Infrastructure.Database
```
