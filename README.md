# EntityCore

`dotnet-crud` - .NET CLI vositasi, CRUD (Create, Read, Update, Delete) operatsiyalarini yaratish uchun mo'ljallangan. Bu tool Entity Framework Core bilan ishlovchi dasturlar uchun tez va oson kodlarni generatsiya qilishga yordam beradi.

## Talablar

- .NET 7.0, 8.0, 9.0 yoki undan yuqori versiya
- Entity Framework Core

## Foydalanish

- `dotnet crud` - bu default buyruq, tool haqida malumot beradi
- `dotnet crud <EntityName>` - du EntityName bo'yicha CRUD service generatsiya qilish uchun
- `dotnet crud <EntityName> --context <DbContextName>` - agar dbContextlar yagona bo'lmasa birini tanlash uchun
- `dotnet crud <EntityName> --controller true` - controller ham generatsiya qilish uchun
