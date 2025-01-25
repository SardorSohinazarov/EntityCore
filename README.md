# EntityCore

## EntityCore Repository TODO

### Immediate Tasks
1. **Command Execution**: Ensure the command builds successfully before execution.
2. **Exception Handling**: Display exceptions clearly and elegantly in red for better debugging and feedback.
3. **Code Cleanup**: Fix `using` directives to remove unresolved references, ensuring no red highlights in the services.
4. **Interface Implementation**: Add interfaces for better abstraction and maintainability.

### DTO and Entity Integration
5. Refactor operations to work seamlessly with DTOs:
   - Use **Entity** + **CreationDto** for creation operations.
   - Use **Entity** + **ModificationDto** for update operations.
   - Ensure operations return **Entity** + **ViewModel** for consistency.

### Enhancements to Core Functionality
6. **Controller Layer**: Add controllers with appropriate logic for CRUD operations and API endpoints.
7. **Filtering and Searching**: Implement a robust and flexible logic for advanced filtering and searching capabilities.
8. **Pagination**: Add pagination to support large data sets efficiently.

---

### Future Improvements (Long-Term Goals)
9. **Repository Layer**: Add repositories with core logic for data access and manipulation:
   - Implement repository patterns for modularity and reusability.
   - Ensure support for advanced querying and filtering.
10. **View Layer**: Add views for front-end functionality:
    - Ensure alignment with back-end structure.
    - Focus on a clean and user-friendly design.
