# PhoneShopApp – Telefonos szaküzlet webalkalmazás

ASP.NET Core MVC alapú üzleti webalkalmazás, amely egy mobiltelefon-szaküzlet készlet- és rendeléskezelési folyamatait valósítja meg.

---

## 1. Funkcionális áttekintés és szerepkörök (RBAC)

Az alkalmazás **ASP.NET Core Identity** alapú hitelesítést és szerepkör-alapú jogosultságkezelést használ:

* **Vendég (nincs bejelentkezve):**
  * Kínálat böngészése, szűrés márka és modellnév alapján.
  * Regisztráció és bejelentkezés.
* **Customer (Vásárló – új regisztrációkor automatikusan kiosztva):**
  * Telefonok rendelése (csak elérhető készlet esetén).
  * Saját korábbi rendelések és azok státuszának megtekintése.
* **Admin (Üzletvezető / Eladó):**
  * Új telefonok felvitele a kínálatba.
  * Meglévő készülékek adatainak, árának és készletmennyiségének módosítása.
  * Beérkezett rendelések kezelése és státuszuk módosítása.

---

## 2. Adatbázis-séma (Entity Framework Core + SQLite)

Az adatkezelés **Code-First** megközelítéssel, **Entity Framework Core** és **SQLite** adatbázis segítségével valósul meg:

* **`Brand` (Márka):** `Id`, `Name` — *1:N kapcsolatban áll a telefonokkal.*
* **`Phone` (Telefon):** `Id`, `ModelName`, `Price`, `StockQuantity`, `Description`, `BrandId` (FK).
* **`Order` (Rendelés):** `Id`, `OrderDate`, `Status`, `TotalAmount`, `UserId` (FK az `AppUser` táblához).
* **`OrderItem` (Rendelési tétel):** `Id`, `OrderId` (FK), `PhoneId` (FK), `Quantity`, `UnitPrice` — *N:M kapcsolat feloldása a rendelések és telefonok között.*
* **`AppUser`:** Az `IdentityUser` osztályból származtatott felhasználói entitás (`FullName` mezővel kiegészítve).

---

## 3. Fő üzleti folyamat: Rendelés és készletkezelés

1. A bejelentkezett vásárló kiválasztja a megvásárolni kívánt készüléket és mennyiséget.
2. A rendszer ellenőrzi, hogy rendelkezésre áll-e a kért mennyiség (`StockQuantity >= Quantity`).
3. Sikeres ellenőrzés esetén a rendelés rögzítésre kerül (`Order` + `OrderItem`), és a készülék raktárkészlete automatikusan csökken egy tranzakción belül (`SaveChangesAsync()`).
4. Rendelés törlése / visszamondása esetén a lefoglalt darabszám visszakerül a készletre.

---

## 4. Telepítés és futtatás

### Előfeltételek
* .NET 10 SDK

### Lépések
1. Klónozd a repository-t, majd lépj be a projekt mappájába:
   ```bash
   git clone https://github.com
   cd Uzleti_Alkalmazas_PhoneShopApp/PhoneShopApp
   ```
2. Hozd létre / frissítsd az SQLite adatbázist a migrációk futtatásával:
   ```bash
   dotnet tool restore
   dotnet ef database update
   ```
3. Indítsd el az alkalmazást:
   ```bash
   dotnet run
   ```
4. Nyisd meg a böngészőben a terminálban kiírt címet (pl. `http://localhost:5xxx`).

---

## 5. Tesztelési adatok (Seed)

Az alkalmazás első indításakor automatikusan létrejönnek a kezdő márkák, telefonok, valamint az alapértelmezett adminisztrátori fiók:

* **Admin e-mail:** `admin@phoneshop.hu`
* **Admin jelszó:** `Admin123`
* *(Vásárlói fiók bármilyen e-mail címmel létrehozható a **Regisztráció** menüpontban.)*

---

## 6. Használt technológiák
* **Backend:** C#, ASP.NET Core MVC (.NET 10)
* **Adatbázis / ORM:** Entity Framework Core, SQLite
* **Biztonság:** ASP.NET Core Identity, Role-Based Access Control (RBAC), Anti-Forgery Token
* **Frontend:** Razor Views, Bootstrap 5
* **Verziókezelés:** Git, GitHub
