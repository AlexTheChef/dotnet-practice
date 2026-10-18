# dotnet-practice

Projekat za ucenje ASP.NET Core-a, MariaDB-a i Docker-a. Lokalno razvojno okruzenje pokrece API i bazu u dva odvojena Linux kontejnera, povezana Compose mrezom.

Komande u ovom dokumentu namenjene su **PowerShell-u na Windowsu**. Sve `docker compose` i `dotnet` komande izvrsavaju se iz foldera projekta, gde su `compose.yaml` i `dotnet-practice.csproj`.

## Tehnologije

| Deo | Verzija ili podesavanje |
| --- | --- |
| ASP.NET Core / C# | .NET 10 (`net10.0`) / C# 14 |
| SDK image za kompajliranje | `mcr.microsoft.com/dotnet/sdk:10.0` |
| Runtime image za API | `mcr.microsoft.com/dotnet/aspnet:10.0` |
| MariaDB image | `mariadb:12.3.3` |
| MySqlConnector | `2.6.2` |
| Swagger / Swashbuckle.AspNetCore | `10.3.0` |

.NET image tagovi `10.0` prate zakrpe unutar verzije 10.0; ne zakljucavaju konkretnu zakrpu ili digest. MariaDB tag je vezan za verziju `12.3.3`.

## Preduslovi

- Docker Desktop, pokrenut sa **Linux containers** i WSL 2 backend-om na Windowsu.
- Docker Compose sa podrskom za Watch (verzija 2.22 ili novija; okruzenje je provereno sa verzijom 5.6.0).
- Git i pristup privatnom repozitorijumu, ako projekat preuzimas sa GitHub-a.
- FlySpeed SQL Query, opciono za pregled baze.
- .NET 10 SDK, samo ako aplikaciju zelis da pokreces direktno na Windowsu. Docker build koristi SDK iz image-a.

U PowerShell-u proveri Docker instalaciju:

```powershell
docker version
docker compose version
```

`docker version` treba da prikaze i Client i Server. Server za ovaj projekat koristi Linux kontejnere.

## Prvo pokretanje

### 1. Preuzmi projekat

U PowerShell-u predji u folder u koji zelis da preuzmes repozitorijum, pa izvrsi:

```powershell
git clone https://github.com/AlexTheChef/dotnet-practice.git
cd dotnet-practice
git switch develop
```

Ako projekat vec imas, samo otvori PowerShell u njegovom folderu. Privatni repozitorijum zahteva prijavu GitHub nalogom koji ima pristup.

### 2. Napravi lokalna podesavanja

Napravi `.env` iz sablona ako taj fajl jos ne postoji:

```powershell
if (-not (Test-Path -LiteralPath .env)) {
    Copy-Item -LiteralPath .env.example -Destination .env
}
notepad .env
```

U fajlu su dva kljuca:

```dotenv
MARIADB_ROOT_PASSWORD=''
MARIADB_PASSWORD=''
```

Izmedju navodnika unesi **dve razlicite duge lozinke** i sacuvaj fajl. Za trenutni connection string koristi lozinku bez znaka `;` i bez navodnika, na primer kombinaciju slova, brojeva i znakova `!`, `#`, `_`, `-`. Slozenije vrednosti zahtevaju odgovarajuce citiranje u connection string-u.

- `MARIADB_ROOT_PASSWORD`: lozinka administratora baze.
- `MARIADB_PASSWORD`: lozinka korisnika `dotnet_app`; koriste je aplikacija i FlySpeed.

Postojeci `.env` zadrzi ako je baza vec inicijalizovana. `.env` je lokalni fajl koji Git ignorise; `.env.example` ostaje prazan sablon za druge developere.

Compose automatski ucitava `.env` iz foldera projekta. Promenljive terminala imaju prednost ako je isti naziv vec postavljen u PowerShell okruzenju. Izraz `${MARIADB_PASSWORD:?Popuni MARIADB_PASSWORD u .env fajlu}` ubacuje vrednost promenljive, a prekida komandu uz navedenu poruku ako je vrednost prazna ili nedostaje. [Docker dokumentacija o promenljivama](https://docs.docker.com/compose/how-tos/environment-variables/variable-interpolation/)

### 3. Proveri konfiguraciju i pokreni kontejnere

```powershell
docker compose config --quiet
```

Uspesna provera ne ispisuje nista. Opcija `--quiet` proverava konfiguraciju bez ispisa connection string-a i lozinki.

Zatim pokreni:

```powershell
docker compose up -d --build --wait
docker compose ps
```

- `--build`: pravi image aplikacije iz `Dockerfile` fajla.
- `-d`: pokrece kontejnere u pozadini.
- `--wait`: ceka da API bude pokrenut i da baza prodje healthcheck.

Prvi build moze da potraje zbog preuzimanja image-a i NuGet paketa. Ocekuj servise `api` sa statusom `Up` i `db` sa statusom `Up ... (healthy)`. API trenutno nema poseban Docker healthcheck; njegovu vezu sa bazom proveravas u sledecem koraku.

### 4. Otvori Swagger i proveri bazu

U pregledaču rucno otvori:

| Namena | Adresa |
| --- | --- |
| Swagger UI | <http://localhost:8080/swagger> |
| Pocetna ruta | <http://localhost:8080/> |
| Provera baze | <http://localhost:8080/db-check> |
| OpenAPI JSON | <http://localhost:8080/swagger/v1/swagger.json> |

U Swagger-u klikni **GET `/db-check` -> Try it out -> Execute**. Ocekuj HTTP 200 i odgovor ovog oblika:

```json
{
  "connected": true,
  "serverVersion": "12.3.3-MariaDB-...",
  "database": "dotnet_practice"
}
```

Pun tekst verzije zavisi od image-a; treba da sadrzi `12.3.3`. Endpoint otvara konekciju i izvrsava `SELECT 1`.

Ista provera iz PowerShell-a:

```powershell
Invoke-RestMethod -Uri 'http://localhost:8080/db-check'
```

Docker API trenutno koristi **lokalni HTTP**. `ASPNETCORE_ENVIRONMENT=Development` omogucava Swagger. `launchSettings.json` koristi se pri lokalnom `dotnet run` pokretanju; Docker pokrece objavljeni DLL preko `ENTRYPOINT` naredbe. HTTPS u kontejneru zahteva zasebno podesavanje sertifikata.

## Svakodnevni razvoj i Watch

Iz foldera projekta pokreni:

```powershell
docker compose up --build --watch
```

Ostavi PowerShell otvoren. Prikazuje logove i prati promene koda; opcija `--watch` koristi se bez `-d`. Kada sacuvas C# fajl, Compose pravi novi image i zamenjuje kontejner aplikacije. Zatim osvezi stranicu ili ponovo izvrsi zahtev kroz Swagger.

Koristimo **Compose Watch sa `action: rebuild`**. Zbog novog build-a i pokretanja ovo je sporije od .NET Hot Reload-a. Pracenje vazi za `api`; pravila iz `.dockerignore` izostavljaju `bin`, `obj`, Git podatke i `.env` fajlove. `compose.yaml` i `README.md` dodatno su izostavljeni iz pracenja. [Docker dokumentacija za Compose Watch](https://docs.docker.com/compose/how-tos/file-watch/)

Posle izmene `compose.yaml` ili `.env` ponovo pokreni Compose da primeni novu konfiguraciju. U Watch prozoru pritisni `Ctrl+C`, pa ponovo izvrsi gornju komandu. Promena lozinke u `.env` ima dodatno ogranicenje opisano u odeljku o podacima baze.

`Ctrl+C` zaustavlja ovu Compose sesiju i kontejnere koje je pokrenula. Podaci baze ostaju u volume-u.

## Povezivanje kroz FlySpeed

Pokreni bazu kroz Compose, pa u FlySpeed-u napravi MySQL/MariaDB konekciju sa ovim vrednostima:

| Polje | Vrednost |
| --- | --- |
| Naziv konekcije | `dotnet-practice Docker` |
| Host / Server | `127.0.0.1` |
| Port | `3307` |
| User / Username | `dotnet_app` |
| Password | Vrednost `MARIADB_PASSWORD` iz `.env`, bez spoljasnjih navodnika |
| Database | `dotnet_practice` |

U SQL editoru koji pripada **toj konekciji** izvrsi:

```sql
SELECT
    VERSION() AS server_version,
    DATABASE() AS database_name,
    @@port AS container_port;
```

Ocekuj verziju sa `12.3.3`, bazu `dotnet_practice` i `container_port = 3306`. Port iz SQL rezultata je port servera unutar kontejnera, dok FlySpeed pristupa preko Windows porta `3307`.

Ako na Windowsu vec postoji MariaDB na portu `3306`, to je zaseban server. Zato Docker bazu objavljujemo na `3307`. Postojeci podaci sa Windows servera ostaju u njegovoj bazi; prenos podataka zahteva zaseban export/import.

## Kako se aplikacija povezuje sa bazom

Compose kreira zajednicku mrezu i omogucava pristup servisima po imenu:

| Odakle pristupas | Odrediste |
| --- | --- |
| Pregledac na Windowsu -> API | `localhost:8080` |
| FlySpeed ili .NET na Windowsu -> Docker baza | `127.0.0.1:3307` |
| API kontejner -> DB kontejner | `db:3306` |

`localhost` unutar API kontejnera oznacava sam API kontejner. Ime `db` odgovara servisu iz `compose.yaml`.

Compose prosledjuje aplikaciji `ConnectionStrings__MariaDb`. .NET prevodi `__` u `:`, pa postojeci `GetConnectionString("MariaDb")` ucitava tu konekciju. Lozinka se ubacuje iz `.env`; User Secrets sa Windowsa nisu potrebni za ovaj nacin pokretanja.

`depends_on` sa `condition: service_healthy` ceka uspesan healthcheck baze pre pokretanja aplikacije. Provera baze koristi `healthcheck.sh --connect --innodb_initialized`.

## Podaci baze i gasenje

Podaci se cuvaju u named volume-u `mariadb-data`, montiranom na `/var/lib/mysql`. Sa podrazumevanim nazivom projekta `dotnet-practice`, Docker volume se obicno zove `dotnet-practice_mariadb-data`.

```powershell
docker compose stop
```

Ovo zaustavlja kontejnere. Ponovo ih pokreni komandom `docker compose up -d --build --wait` ili razvojnom komandom sa `--watch`.

```powershell
docker compose down
```

Ovo zaustavlja i uklanja kontejnere i Compose mrezu, a cuva named volume. Sledeci `up` ponovo koristi sacuvane podatke.

**`docker compose down -v` uklanja i volume sa podacima baze.** Koristi ga samo za nameran reset lokalne baze. [Docker dokumentacija za `down`](https://docs.docker.com/reference/cli/docker/compose/down/)

MariaDB kreira bazu, korisnika i njihove lozinke **pri prvom pokretanju sa praznim data direktorijumom**. Naknadna promena `.env` fajla ne menja lozinku vec postojeceg korisnika u sacuvanoj bazi. Takva promena zahteva i odgovarajucu izmenu korisnika u samoj bazi. [MariaDB dokumentacija o inicijalizaciji](https://mariadb.com/docs/server/server-management/automated-mariadb-deployment-and-administration/docker-and-mariadb/mariadb-server-docker-official-image-environment-variables)

Volume omogucava cuvanje podataka kroz zamenu kontejnera. Za rezervnu kopiju vrednih podataka potreban je zaseban backup/export.

## Korisne komande

Sve komande pokreni u PowerShell-u iz foldera projekta.

| Komanda | Namena |
| --- | --- |
| `docker compose up --build --watch` | Razvoj uz logove i automatski rebuild |
| `docker compose up -d --build --wait` | Pokretanje u pozadini |
| `docker compose up -d --wait db` | Pokretanje samo baze |
| `docker compose ps` | Stanje pokrenutih servisa i portovi |
| `docker compose ps -a` | Stanje svih servisa, ukljucujuci zaustavljene |
| `docker compose logs -f api` | Pracenje logova aplikacije; `Ctrl+C` prekida samo pracenje logova |
| `docker compose logs --tail 100 db` | Poslednjih 100 redova loga baze |
| `docker compose config --quiet` | Provera Compose konfiguracije |
| `docker build -t dotnet-practice:local .` | Pravljenje image-a bez pokretanja aplikacije |
| `docker image ls dotnet-practice` | Pregled lokalnog image-a aplikacije |
| `docker compose stop` | Zaustavljanje servisa |
| `docker compose down` | Uklanjanje kontejnera i mreze, uz cuvanje named volume-a |

## Opciono: aplikacija na Windowsu, baza u Dockeru

Ovaj nacin rada koristi lokalni .NET 10 SDK i `dotnet watch run` sa Hot Reload-om.

Pokreni bazu iz foldera projekta:

```powershell
docker compose up -d --wait db
```

Lokalnoj aplikaciji podesi User Secret. **Pre izvrsavanja zameni `ZAMENI_LOZINKOM_IZ_ENV` vrednoscu `MARIADB_PASSWORD` iz svog `.env` fajla.**

```powershell
dotnet user-secrets set 'ConnectionStrings:MariaDb' 'Server=127.0.0.1;Port=3307;Database=dotnet_practice;User ID=dotnet_app;Password=ZAMENI_LOZINKOM_IZ_ENV;'
```

Projekat vec ima `UserSecretsId`; `dotnet user-secrets init` nije potreban. User Secrets se cuva izvan repozitorijuma, vezan je za Windows korisnika i ucitava se u `Development` okruzenju. Namenjen je lokalnom razvoju i ne sifruje sacuvane vrednosti. Lokalno pokrenuta aplikacija ne ucitava Compose `.env` automatski. [Microsoft dokumentacija za User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0)

Za lokalni HTTPS jednom podesi poverenje u razvojni sertifikat, pa pokreni aplikaciju:

```powershell
dotnet dev-certs https --trust
dotnet watch run
```

Podrazumevani profil je `https`. Swagger je na <https://localhost:7013/swagger>, a isti profil omogucava i HTTP na <http://localhost:5071/swagger>. Ovo podesavanje sertifikata vazi za lokalno pokretanje; Docker API ostaje na <http://localhost:8080/swagger>.

## Ako nesto ne radi

- **Docker ne moze da se poveze sa engine-om:** otvori Docker Desktop, sacekaj da bude spreman i proveri `docker version`.
- **Poruka `Popuni MARIADB_...`:** proveri da `.env` postoji u folderu projekta i da su obe vrednosti popunjene.
- **API se ne otvara ili je zaustavljen:** izvrsi `docker compose ps -a` i `docker compose logs --tail 100 api`. Posle ispravke pokreni Compose ponovo.
- **Port je zauzet:** proveri `Get-NetTCPConnection -State Listen -LocalPort 8080,3307`. U `ports` podesavanju levi port pripada Windowsu; desni pripada kontejneru. Posle promene levog porta koristi novu adresu u pregledacu ili FlySpeed-u.
- **Baza prijavljuje `Access denied`:** proveri korisnika `dotnet_app` i `MARIADB_PASSWORD`, kao i da li su podaci u volume-u inicijalizovani ranijom lozinkom.
- **FlySpeed prikazuje verziju druge MariaDB instalacije:** proveri da SQL tab koristi Docker konekciju sa `127.0.0.1:3307` i bazom `dotnet_practice`.
- **Promena koda se ne vidi:** koristi `docker compose up --build --watch`, sacuvaj fajl i sacekaj zavrsetak rebuild-a. Pokretanje samo sa `-d` ne ukljucuje pracenje promena.
- **Swagger vraca 404:** proveri URL i `ASPNETCORE_ENVIRONMENT: Development` u servisu `api`.

## Fajlovi projekta

| Fajl | Uloga |
| --- | --- |
| `Program.cs` | Swagger, konekcija sa bazom i API rute |
| `dotnet-practice.csproj` | Target framework, NuGet paketi i UserSecretsId |
| `Dockerfile` | SDK build faza i zavrsni ASP.NET runtime image; API radi kao korisnik `app` |
| `compose.yaml` | Servisi, mreza, portovi, konekcija, volume, healthcheck i Watch |
| `.dockerignore` | Fajlovi izostavljeni iz Docker build konteksta i Watch-a |
| `.env.example` | Prazan sablon lokalnih lozinki koji ulazi u Git |
| `.env` | Tvoje lokalne vrednosti koje Git ignorise |
| `.gitignore` | Lokalni, generisani i osetljivi fajlovi izostavljeni iz Gita |
| `Properties/launchSettings.json` | Profili i portovi za lokalni `dotnet run` / `dotnet watch run` |

Ovo je konfiguracija za lokalni razvoj: Swagger je ukljucen, API koristi lokalni HTTP, a portovi su vezani za `127.0.0.1`. Podesavanje za objavljivanje na serveru radi se zasebno.
