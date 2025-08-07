# 🧬 Family Tree System

**Family Tree System** — bu foydalanuvchilarga o‘z oilaviy daraxtlarini qurish, har bir a’zoni aniqlik bilan kiritish va vizual tarzda ko‘rish imkonini beruvchi veb-ilova.

---

## 📚 Maqsad

- Shaxslar o‘z oilasini yaratib, unga a’zolarni (ota, ona, farzandlar) qo‘shadi.
- Har bir `Member` o‘ziga xos ma’lumotlarga ega: tug‘ilgan sanasi, vafot sanasi, tavsif, jins, ota-onasi, turmush o‘rtog‘i va bolalari.
- Har bir `Family` o‘z foydalanuvchilari, rollari va huquqlari bilan alohida boshqariladi.

---

## 🧱 Ma’lumotlar bazasi tuzilmasi

### 🔹 Table: `Family`
| Field       | Type     | Description                     |
|-------------|----------|---------------------------------|
| `Id`        | GUID     | Primary key                     |
| `Name`      | String   | Family full name                |
| `Description` | String | Family description              |
| `FamilyName`| String   | Unique, for public sharing link |
| `Image`     | URL      | Logo or banner                  |
| `Users`     | List     | Users linked to this family     |
| `Members`   | List     | Family tree members             |

---

### 🔹 Table: `Member`
| Field       | Type     | Description                       |
|-------------|----------|-----------------------------------|
| `Id`        | GUID     | Primary key                       |
| `FirstName` | String   | Member's first name               |
| `LastName`  | String   | Member's last name                |
| `Description`| String  | Notes about the member            |
| `Gender`    | Enum     | Male / Female                     |
| `FamilyId`  | GUID     | Foreign key → Family.Id           |
| `BirthDay`  | Date     | Date of birth                     |
| `DeathDay`  | Date?    | Optional date of death            |
| `Father`    | Member?  | FK → Member                       |
| `Mother`    | Member?  | FK → Member                       |
| `Spouse`    | Member?  | FK → Member                       |
| `Children`  | List     | Reverse navigation                |

> ⚠️ `Member` bog‘lanishlar o‘z ichida o‘ziga havola (`self-reference`) qiladi.

---

### 🔹 Table: `User`
| Field       | Type     | Description                |
|-------------|----------|----------------------------|
| `Id`        | GUID     | Primary key                |
| `FirstName` | String   | User's first name          |
| `LastName`  | String   | User's last name           |
| `UserName`  | String   | Unique username            |
| `Phone`     | Phone    | Unique phone number        |
| `Email`     | Email    | Unique email address       |
| `Password`  | Hash     | Hashed password            |
| `Family`    | FK       | FK → Family.Id             |
| `Role`      | FK       | FK → Role.Id               |

---

### 🔹 Table: `Role`
| Field          | Type     | Description             |
|----------------|----------|-------------------------|
| `Id`           | GUID     | Primary key             |
| `Name`         | String   | Role name (e.g., Admin) |
| `Description`  | String   | Role description        |
| `DesignedName` | String   | System-defined name     |
| `Family`       | FK       | FK → Family.Id          |
| `Permissions`  | List     | Enum list               |

---

### 🔹 Enum: `Permission`
```ts
enum Permission {
  GetFamily, CreateFamily, UpdateFamily, DeleteFamily,
  GetMember, CreateMember, UpdateMember, DeleteMember,
  GetUser, UpdateUser, DeleteUser,
  GetRole, CreateRole, UpdateROle, DeleteRole
}
