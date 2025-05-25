# EntityCore

`dotnet-crud` - .NET CLI vositasi, CRUD (Create, Read, Update, Delete) operatsiyalarini yaratish uchun mo'ljallangan. Bu tool Entity Framework Core bilan ishlovchi dasturlar uchun tez va oson kodlarni generatsiya qilishga yordam beradi. Admin panelingizni commandalar orqali bir zumda generatsiya qildiring.

## Talablar

- .NET 8.0, 9.0 yoki undan yuqori versiya
- Entity Framework Core

## Foydalanish

- `dotnet crud` - bu default buyruq, tool haqida malumot beradi
- `dotnet crud --service <EntityName>` - du EntityName bo'yicha CRUD service generatsiya qilish uchun
- `dotnet crud --service <EntityName> --context <DbContextName>` - agar dbContextlar yagona bo'lmasa birini tanlash uchun
- `dotnet crud --controller <EntityName>` - controller ham generatsiya qilish uchun
- `dotnet crud --dto <EntityName>` - dto ham generatsiya qilish uchun
- `dotnet crud --view <EntityName>` - blazor view ham generatsiya qilish uchun
