# Rezervacije i prodaja karata: kontekst za nastavak rada

Poslednje azuriranje: 2026-10-19.

Ovaj dokument prenosi dogovore iz razgovora o izboru projekta. Procitati ga na
pocetku novog chata, zajedno sa `README.md` i trenutnim kodom. Azurirati status i
vazne odluke kada se neka celina zavrsi. Planirane funkcije nisu implementirane.

## Cilj i dogovori

- Korisnik uci backend i zeli ubrzano, ali postepeno ucenje kroz stvaran projekat.
- Izabran je sistem za rezervacije i prodaju karata za dogadjaje.
- Korisnik prihvata da je tema popularna. Prioritet su kvalitet implementacije,
  razumevanje backend-a i vestine koje moze da objasni na tehnickom intervjuu.
- Cilj je zaokruzen portfolio proizvod sa ozbiljno obradjenim transakcijama,
  konkurentnoscu, poslovnim pravilima, bezbednoscu i oporavkom od gresaka.
- Radni arhitektonski pravac je modularni monolit: jedna aplikacija sa jasnim
  poslovnim celinama. Dodatna infrastruktura uvodi se kada resava konkretan problem.
- Za svaku celinu objasniti problem, tok podataka, implementaciju i proveru.
  Razgovor i objasnjenja voditi na srpskom.

## Trenutno stanje repozitorijuma

Provereno citanjem fajlova 2026-10-19:

- ASP.NET Core aplikacija cilja .NET 10, sa ukljucenim nullable reference tipovima.
- Pristup bazi trenutno koristi MySqlConnector. ORM nije izabran.
- Swagger je podesen za razvojno okruzenje.
- `Program.cs` sadrzi samo pocetnu rutu `/` i `/db-check`, koji izvrsava `SELECT 1`.
- Postoje `Dockerfile` i `compose.yaml`, sa servisima `api` i `db` (MariaDB).
- Compose definise trajni volume baze, njen healthcheck i pracenje promena koda.
- API je konfigurisan na `http://localhost:8080`, a baza na host portu `3307`.
- `README.md` opisuje lokalno podesavanje, pokretanje i rad sa Docker-om.
- Funkcionalnosti za dogadjaje, korisnike, rezervacije i kupovinu jos ne postoje.
- Tokom prenosa konteksta nisu pokretani aplikacija, Docker servisi ili testovi;
  prethodni setup ne treba predstavljati kao novu runtime proveru.

## Proizvod koji gradimo

Organizator objavljuje dogadjaj sa ogranicenim kapacitetom. Kupac privremeno
rezervise karte, zavrsava placanje i dobija kartu za ulazak. Osoblje proverava
kartu na ulazu. Nezavrsene rezervacije isticu i oslobadjaju kapacitet.

Prva celina je dogadjaj sa ogranicenim kapacitetom i rezervacija jedne karte.
Numerisana sedista, tacno trajanje rezervacije i detalji naplate tek treba da se
definisu. Ranije pomenutih deset minuta bilo je primer, a ne usvojeno pravilo.

## Faze razvoja

1. **Osnova proizvoda:** model dogadjaja, kapacitet, sema baze i migracije,
   validacija zahteva i jasni API odgovori.
2. **Pouzdane rezervacije:** transakcije, konkurentni zahtevi, istek i otkazivanje,
   idempotentnost i testovi ispravnosti nad pravom bazom.
3. **Kompletna kupovina:** korisnici i dozvole, placanje u test okruzenju,
   obrada webhook potvrda, izdavanje i provera karata.
4. **Rad i odrzavanje:** pouzdani poslovi u pozadini, testovi opterecenja,
   logovi i metrike, automatizovane provere i objavljivanje aplikacije.

## Dokazi kvaliteta koje zelimo da napravimo

Ovo su ciljevi za buducu implementaciju i testove, a ne vec dobijeni rezultati.

- Sa kapacitetom od 10 karata i 100 istovremenih zahteva za po jednu kartu,
  bez isteka i otkazivanja tokom testa, uspeva tacno 10 rezervacija. Proveriti
  i odgovore API-ja i trajno stanje baze.
- Aktivne rezervacije i prodate karte zajedno ne premasuju kapacitet dogadjaja.
- Ponovljeni zahtev sa istim idempotency kljucem ne pravi dodatnu rezervaciju.
  Definisati i ponasanje kada se isti kljuc ponovi sa drugacijim podacima.
- Istovremeni istek i potvrda rezervacije daju jedno dosledno poslovno stanje.
- Duplirane ili zakasnele potvrde placanja ne izdaju dodatne karte. Posebno
  definisati sta radimo kada uplata stigne nakon isteka rezervacije.
- Restart aplikacije ne gubi trajno prihvacene poslove za naknadnu obradu.
- Kupac moze da pristupi samo svojim rezervacijama, a organizator svojim
  dogadjajima. Provera ovih pravila mora da postoji na backend-u.
- Merenja odziva i opterecenja navode uslove testa, ogranicenja i stvarne rezultate.
- Tehnicke odluke beleze problem, razmotrene opcije, razlog izbora i proveru.

## Sledeci zadatak

Otvoriti celinu "Dogadjaji i model baze". Proci kroz prvi konkretan primer:
dogadjaj sa 10 mesta i rezervacija jedne karte. Objasniti put HTTP zahtev ->
validacija -> poslovno pravilo -> SQL transakcija -> HTTP odgovor. Definisati
minimalna polja dogadjaja i rezervacije i pravilo raspolozivog kapaciteta pre
postepene implementacije. Konkurentnost treba uzeti u obzir vec pri projektovanju
prve rezervacije.

## Nastavak u drugim chatovima

Za rad na kodu novi chat treba da ima pristup ovom lokalnom repozitorijumu.
Ako se planiranje nastavlja u ChatGPT projektu bez pristupa lokalnom folderu,
priloziti ovaj fajl kao izvor i osveziti ga posle novih odluka.

Pocetna poruka za novi chat:

> Procitaj docs/PROJECT.md, README.md i trenutno stanje koda. Nastavljamo sistem
> za rezervacije i prodaju karata. Ucim backend; vodi me kroz manje korake i
> objasni razloge vaznih odluka. Krecemo od sledeceg zadatka opisanog u dokumentu.
