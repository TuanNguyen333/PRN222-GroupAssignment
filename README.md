# PRN222-GroupAssignment

## Package Structures

### Backend
```
PRN222-GroupAssignment/
│
├── src/
│   ├── Application/                			# Controller Layer (Web API)
│   │   ├── Controllers/
│   │   │   ├── CategoryController.cs
│   │   │   ├── MemberController.cs
│   │   └── Program.cs
│   │
│   ├── BusinessObject/         				# BBL (Services)
│   │   ├── Base/
│   │   │   ├── ICrudService.cs
│   │   ├── DTOs/
│   │   │   ├── Categories/
│   │   │   ├── Members/
│   │   ├── Mappers/
│   │   ├── Services/
│   │       ├── CategoryService.cs
│   │       ├── MemberService.cs
│   │
│   └── DataAccess/             	 			# DAL (Repositories)
│       ├── Base/
│       │   ├── ICrudRepository.cs
│       ├── Data/
│       │   ├── EStoreDbContext.cs
│       ├── Entities/
│       │   ├── Category.cs
│       │   ├── Member.cs
│       ├── Repositories/
│       │   ├── ICategoryRepository.cs
│       │   ├── IMemberRepository.cs
│       ├── Repositories.Impl/
│           ├── CategoryRepository.cs
│           ├── MemberRepository.cs
│
├── .gitignore
├── Ass03Solution.sln
└── README.md
```

### Frontend
```
PRN222-GroupAssignment/
│
├── src/
     ├── eStore/                			# Frontend (Blazor)
         ├── Components/
         │   ├── Layout/
         │   ├── Pages/
         └── Program.cs
```