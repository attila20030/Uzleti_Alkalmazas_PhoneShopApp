# Fejlesztői és Felhasználói Dokumentáció
## PhoneShopApp – Mobiltelefon-szaküzlet üzleti webalkalmazás

* **Készítette:** Daroczi Attila
* **Tárgy / Téma:** Üzleti alkalmazások fejlesztése – Féléves beadandó feladat
* **Forráskód (GitHub repository):** https://github.com/attila20030/Uzleti_Alkalmazas_PhoneShopApp
* **Technológiai környezet:** C# / ASP.NET Core MVC (.NET 10.0), Entity Framework Core, SQLite, Bootstrap 5
* **Dátum:** 2026. november 26.

---

## 1. Rendszerspecifikáció és célkitűzés

A **PhoneShopApp** egy ASP.NET Core MVC alapú, relációs adatbázisra épülő üzleti webalkalmazás, amely egy mobiltelefon-szaküzlet napi működését, készletnyilvántartását és online értékesítési folyamatait támogatja.

### 1.1. Megoldott üzleti probléma
Egy kiskereskedelmi szaküzlet működésében kritikus fontosságú, hogy a kínálatban szereplő készülékek elérhetősége (raktárkészlete) és árazása naprakész legyen, a vásárlók valós idejű készletinformációk alapján adhassanak le megrendeléseket, az üzletvezetés pedig elkülönített adminisztrációs felületen kezelhesse a termékkínálatot és a beérkező rendelések életciklusát.

### 1.2. Főbb funkcionális követelmények
1. **Nyitóoldal és márkaválasztó:** Áttekinthető kezdőlap kiemelt márkakártyákkal (Apple, Samsung, Xiaomi) és közvetlen navigációval.
2. **Termékkatalógus, keresés és szűrés:** Készülékek listázása márka, modellnév, egységár és aktuális raktárkészlet szerint, dinamikus szűréssel.
3. **Felhasználókezelés (Regisztráció, Bejelentkezés, Kijelentkezés):** ASP.NET Core Identity alapú fiókkezelés egyedi e-mail címmel és jelszószabályokkal.
4. **Szerepkör-alapú jogosultságkezelés (RBAC):** Vendég, Vásárló (`Customer`) és Üzletvezető (`Admin`) jogosultsági szintek szigorú elválasztása mind vezérlő (Controller), mind nézet (View) szinten.
5. **Rendelési folyamat és tranzakcionális készletkezelés:** Megrendelés leadása készletellenőrzéssel, a raktárkészlet automatikus csökkentése vásárláskor, illetve visszatöltése rendelés törlésekor.
6. **Adminisztrációs műveletek:** Új készülékek felvitele, meglévő termékek adatainak módosítása, valamint az összes rendelés státuszának kezelése.

### 1.3. Nem-funkcionális követelmények és biztonság
* **Adatintegritás:** A rendelés rögzítése és a készlet csökkentése egyetlen adatbázis-tranzakcióban (`SaveChangesAsync()`) történik, így nem fordulhat elő inkonzisztens készletállapot.
* **CSRF védelem:** Minden adatot módosító HTTP POST kérés `[ValidateAntiForgeryToken]` attribútummal van védve.
* **Reszponzív megjelenés:** A Bootstrap 5 keretrendszer biztosítja az asztali és mobil nézetek közötti akadálymentes megjelenítést.

---

## 2. Rendszerarchitektúra és könyvtárstruktúra

Az alkalmazás a klasszikus **MVC (Model–View–Controller)** tervezési mintát követi, amely tisztán szétválasztja az adatmodellt, az üzleti logikát és a megjelenítési réteget:

```text
PhoneShopApp/
├── Controllers/
│   ├── AccountController.cs   # Regisztráció, bejelentkezés, kijelentkezés
│   ├── HomeController.cs      # Nyitóoldal megjelenítése
│   ├── OrdersController.cs    # Rendelés leadása, listázása, státuszkezelés
│   └── PhonesController.cs    # Telefonok listázása, szűrése és Admin CRUD
├── Data/
│   └── AppDbContext.cs        # EF Core adatbázis-kontextus és kezdőadatok (Seed)
├── Migrations/                # Code-First adatbázis-migrációk (InitialCreate, AddOrders)
├── Models/
│   ├── AppUser.cs             # IdentityUser bővítése FullName tulajdonsággal
│   ├── Brand.cs               # Telefonmárka entitás
│   ├── Order.cs               # Rendelés fejléc entitás
│   ├── OrderItem.cs           # Rendelési tétel kapcsolóentitás
│   └── Phone.cs               # Mobiltelefon entitás validációs attribútumokkal
├── Views/
│   ├── Account/               # Login.cshtml, Register.cshtml
│   ├── Home/                  # Index.cshtml (nyitóoldal)
│   ├── Orders/                # Index.cshtml (saját / összes rendelés)
│   ├── Phones/                # Index.cshtml, Create.cshtml, Edit.cshtml
│   └── Shared/                # _Layout.cshtml (dinamikus menüsáv)
├── Program.cs                 # DI regisztráció, Identity, szerepkörök és Admin seed
├── appsettings.json           # SQLite kapcsolati lánc és SQL naplózás
└── phoneshop.db               # SQLite adatbázisfájl
```

---

## 3. Adatbázis-séma és entitáskapcsolatok (ER-modell)

Az adatbázis **Entity Framework Core Code-First** megközelítéssel épül fel. A `Program.cs` és az `AppDbContext.cs` gondoskodik arról, hogy az alkalmazás indításakor a törzsadatok és az adminisztrátori fiók automatikusan rendelkezésre álljanak.

### 3.1. Entitások részletes leírása

#### `Brands` (Márkák)
* `Id` (`int`, PK): Egyedi azonosító.
* `Name` (`string`): A gyártó neve (alapértelmezetten: *Apple*, *Samsung*, *Xiaomi*).
* *Kapcsolat:* `1:N` kapcsolat a `Phones` táblával (`List<Phone> Phones`).

#### `Phones` (Mobiltelefonok)
* `Id` (`int`, PK): Egyedi azonosító.
* `ModelName` (`string`, kötelező): A készülék pontos típusa (`[Required]`).
* `Price` (`decimal`): Egységár forintban (`[Range(1, 5000000)]`).
* `StockQuantity` (`int`): Elérhető raktárkészlet darabszáma (`[Range(0, 10000)]`).
* `Description` (`string?`): Rövid műszaki leírás (tárhely, szín, fő jellemzők).
* `BrandId` (`int`, FK): Hivatkozás a gyártó márkára (`Brands.Id`).

#### `AspNetUsers` / `AppUser` (Felhasználók)
* Az `IdentityUser` osztályból származik, kiegészítve a `FullName` (`string`) mezővel.
* Tárolja a titkosított jelszó-hasht (`PasswordHash`), az e-mail címet (`Email`) és a felhasználónevet (`UserName`).

#### `Orders` (Rendelések)
* `Id` (`int`, PK): Rendelés sorszáma.
* `OrderDate` (`DateTime`): A megrendelés rögzítésének pontos időpontja.
* `Status` (`string`): A rendelés aktuális állapota (*„Feldolgozás alatt”*, *„Teljesítve”*, *„Törölve”*).
* `TotalAmount` (`decimal`): A rendelés végösszege forintban.
* `UserId` (`string`, FK): A megrendelő felhasználó azonosítója (`AspNetUsers.Id`).

#### `OrderItems` (Rendelési tételek)
* `Id` (`int`, PK): Egyedi tételazonosító.
* `OrderId` (`int`, FK): Hivatkozás a rendelésre (`Orders.Id`).
* `PhoneId` (`int`, FK): Hivatkozás a megrendelt készülékre (`Phones.Id`).
* `Quantity` (`int`): Megrendelt darabszám.
* `UnitPrice` (`decimal`): A vásárlás pillanatában érvényes egységár (megóvja a korábbi rendeléseket a későbbi árváltozásoktól).

### 3.2. Kapcsolati összefoglaló
* **Márka – Telefon (`1:N`):** Egy márkához több telefon tartozik, minden készülék pontosan egy márkához kapcsolódik.
* **Felhasználó – Rendelés (`1:N`):** Egy regisztrált vásárló több rendelést adhat le.
* **Rendelés – Telefon (`N:M`):** Az `OrderItems` kapcsolótábla oldja fel a sok-a-sokhoz kapcsolatot.

---

## 4. Felhasználói szerepkörök és jogosultsági rendszer (RBAC)

Az alkalmazás három felhasználói csoportot különböztet meg, amelyek eltérő menüpontokat és műveleteket érhetnek el:

| Funkció / Művelet | Vendég (Nincs belépve) | Vásárló (`Customer`) | Üzletvezető (`Admin`) |
| :--- | :---: | :---: | :---: |
| Kezdőlap és kínálat böngészése | Igen | Igen | Igen |
| Szűrés márka és modellnév alapján | Igen | Igen | Igen |
| Regisztráció és bejelentkezés | Igen | – | – |
| Telefon megrendelése (készletig) | Nem | Igen | Igen |
| Saját rendelések megtekintése | Nem | Igen | Igen |
| Összes vásárlói rendelés megtekintése | Nem | Nem | Igen |
| Rendelés státuszának módosítása / törlése | Nem | Nem | Igen |
| Új telefon felvitele és szerkesztése | Nem | Nem | Igen |

### 4.1. Szerepkörök kiosztása és védelme
* **Automatikus szerepkör-hozzárendelés:** Minden új felhasználó, aki a `/Account/Register` felületen regisztrál, automatikusan megkapja a `Customer` szerepkört.
* **Kezdő adminisztrátor:** A `Program.cs` indításkor ellenőrzi az `Admin` és `Customer` szerepkörök meglétét, és létrehozza az alapértelmezett `admin@phoneshop.hu` felhasználót `Admin` jogosultsággal.
* **Vezérlő szintű védelem:**
  * A `PhonesController` osztály szinten `[Authorize(Roles = "Admin")]` védelemmel rendelkezik, míg az `Index` listaoldal `[AllowAnonymous]` attribútummal bárki számára nyitott.
  * Az `OrdersController` osztály szinten `[Authorize]` attribútummal védett, a státuszmódosító `UpdateStatus` metódus pedig kizárólag `[Authorize(Roles = "Admin")]` jogosultsággal hívható meg.

---

## 5. Az alkalmazás működése és fő üzleti folyamatai

### 5.1. Böngészés és dinamikus szűrés (`/Phones`)
A kínálat oldalon az `AppDbContext` az `.Include(p => p.Brand)` hívással betölti a gyártók nevét is. A felhasználó legördülő listából választhat márkát (`brandId`), illetve szöveges keresőmezőben szűrhet a modell nevére (`search`). A rendszer színkódolt jelvényekkel jelzi a készletállapotot: zöld jelvény mutatja a rendelkezésre álló darabszámot, piros jelvény pedig az elfogyott készülékeket.

### 5.2. Rendelés leadása és készletcsökkentés (`OrdersController.Create`)
1. A bejelentkezett felhasználó a **Telefonok** táblázatban kiválasztja a kívánt darabszámot (1 és az elérhető készlet között), majd a **Megrendelem** gombra kattint.
2. A szerveroldali üzleti logika ellenőrzi, hogy a kért darabszám nem haladja-e meg a raktárkészletet (`phone.StockQuantity < quantity`). Készlethiány esetén hibaüzenettel (`TempData["Error"]`) visszairányítja a felhasználót.
3. Megfelelő készlet esetén létrejön az `Order` és a hozzá tartozó `OrderItem` rekord, a `phone.StockQuantity` értéke csökken a megrendelt mennyiséggel, majd a változások egy tranzakcióban mentésre kerülnek.
4. A rendszer átirányítja a vásárlót a **Rendelések** (`/Orders`) oldalra, ahol megjelenik a visszaigazolás.

### 5.3. Rendeléskezelés és készlet-visszatöltés (`OrdersController.UpdateStatus`)
* Az **Admin** szerepkörű felhasználó a **Rendelések** menüpontban az összes felhasználó megrendelését látja (a vásárló nevével és e-mail címével együtt).
* A **Teljesítve** gomb megnyomásakor a rendelés státusza *„Teljesítve”* állapotra vált.
