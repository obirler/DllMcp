# TestAssembly

A comprehensive sample class library designed to test the MCP-DLL Document Proxy Server's ability to extract metadata, parse XML documentation, and handle various .NET constructs.

## Purpose

This assembly serves as test data for validating:
- Assembly loading and metadata extraction
- XML documentation parsing and mapping
- ECMA-335 ID generation for different member types
- Search and pagination functionality
- MCP tool responses

## Included Types

### 1. Calculator (Class)
A sample calculator with basic arithmetic operations.

**Members:**
- `Add(int, int)` - Addition with XML documentation including example
- `Subtract(int, int)` - Subtraction operation
- `Multiply(int, int)` - Multiplication with private helper method
- `Divide(int, int)` - Division with exception documentation
- `CurrentValue` property - Simple property for state storage

**Purpose:** Tests method documentation, properties, and exception handling.

### 2. StringHelper (Static Class)
Utility class for string manipulation.

**Members:**
- `Reverse(string)` - String reversal
- `ToUpperCase(string)` - Case conversion
- `IsPalindrome(string)` - Palindrome detection

**Purpose:** Tests static classes and methods with null handling.

### 3. Person (Class)
A domain model representing a person.

**Members:**
- `FirstName`, `LastName` properties
- `FullName` computed property
- `Age` property
- Parameterless and parameterized constructors
- `ToString()` override

**Purpose:** Tests classes with multiple constructors, properties, and inheritance.

### 4. IRepository<T> (Generic Interface)
A generic repository interface for data access.

**Members:**
- `GetById(int)` - Retrieve by ID
- `GetAll()` - Get all items
- `Add(T)` - Add new item
- `Delete(int)` - Delete by ID

**Purpose:** Tests generic interfaces and method signatures.

### 5. DayOfWeek (Enum)
Days of the week enumeration.

**Members:**
- Sunday through Saturday with values 0-6

**Purpose:** Tests enum documentation and member extraction.

### 6. Point (Struct)
A 2D point structure.

**Members:**
- `X`, `Y` properties
- Constructor
- `DistanceTo(Point)` - Distance calculation

**Purpose:** Tests value types (structs) and mathematical operations.

### 7. ValueChangedEventArgs (Class)
Event arguments for value change notifications.

**Members:**
- `OldValue`, `NewValue` properties
- Constructor

**Purpose:** Tests event argument classes and EventArgs inheritance.

### 8. Counter (Class)
A counter with event notifications.

**Members:**
- `ValueChanged` event
- `Value` property that triggers event
- `Increment()`, `Decrement()` methods
- Protected `OnValueChanged(EventArgs)` method

**Purpose:** Tests events, protected members, and event handling patterns.

## Building

```bash
dotnet build -c Release
```

## Output

After building, the following files are generated:
- `TestAssembly.dll` - The compiled assembly
- `TestAssembly.xml` - XML documentation file with all member documentation

## Documentation Quality

All public types and members include XML documentation with:
- `<summary>` - Brief description
- `<param>` - Parameter descriptions
- `<returns>` - Return value descriptions
- `<exception>` - Exception documentation where applicable
- `<example>` - Usage examples where helpful

## Usage in Tests

The compiled DLL and XML are used by:
1. **Integration tests** (`test/integration-test.sh`) - Upload and verify assembly indexing
2. **API tests** - Test search, pagination, and metadata extraction
3. **Manual testing** - Example assembly for UI testing

## Test Coverage

This assembly covers:
- ✅ Classes (regular, static, with events)
- ✅ Interfaces (generic and non-generic)
- ✅ Structs (value types)
- ✅ Enums (with explicit values)
- ✅ Methods (public, private, static)
- ✅ Properties (get/set, computed, auto-implemented)
- ✅ Fields (public, private)
- ✅ Events (custom EventArgs)
- ✅ Constructors (parameterless, parameterized)
- ✅ Generic type parameters
- ✅ Exception handling documentation
- ✅ Usage examples in documentation

## Example Member IDs Generated

The assembly generates ECMA-335 IDs like:
- `T:TestAssembly.Calculator` - Calculator class
- `M:TestAssembly.Calculator.Add(System.Int32,System.Int32)` - Add method
- `P:TestAssembly.Person.FullName` - FullName property
- `F:TestAssembly.Counter._value` - Private field
- `E:TestAssembly.Counter.ValueChanged` - ValueChanged event

## Adding New Test Cases

To add new test types:
1. Add the class/interface/struct to `Class1.cs`
2. Include comprehensive XML documentation
3. Rebuild in Release mode
4. Verify XML file is generated
5. Run integration tests to validate
