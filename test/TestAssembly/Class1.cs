namespace TestAssembly;

/// <summary>
/// A sample calculator class for testing the MCP-DLL Document Proxy Server.
/// </summary>
public class Calculator
{
    /// <summary>
    /// Adds two integers and returns the result.
    /// </summary>
    /// <param name="a">The first number to add.</param>
    /// <param name="b">The second number to add.</param>
    /// <returns>The sum of the two numbers.</returns>
    /// <example>
    /// <code>
    /// var calc = new Calculator();
    /// int result = calc.Add(5, 3); // Returns 8
    /// </code>
    /// </example>
    public int Add(int a, int b)
    {
        return a + b;
    }

    /// <summary>
    /// Subtracts the second integer from the first.
    /// </summary>
    /// <param name="a">The number to subtract from.</param>
    /// <param name="b">The number to subtract.</param>
    /// <returns>The difference between the two numbers.</returns>
    public int Subtract(int a, int b)
    {
        return a - b;
    }

    /// <summary>
    /// Gets or sets the current value stored in the calculator.
    /// </summary>
    public int CurrentValue { get; set; }

    /// <summary>
    /// Multiplies two numbers using a private helper method.
    /// </summary>
    /// <param name="a">First factor.</param>
    /// <param name="b">Second factor.</param>
    /// <returns>The product of the two numbers.</returns>
    public int Multiply(int a, int b)
    {
        return MultiplyInternal(a, b);
    }

    /// <summary>
    /// Divides two numbers.
    /// </summary>
    /// <param name="dividend">The number to be divided.</param>
    /// <param name="divisor">The number to divide by.</param>
    /// <returns>The quotient of the division.</returns>
    /// <exception cref="DivideByZeroException">Thrown when divisor is zero.</exception>
    public double Divide(int dividend, int divisor)
    {
        if (divisor == 0)
            throw new DivideByZeroException("Cannot divide by zero.");
        return (double)dividend / divisor;
    }

    private int MultiplyInternal(int x, int y)
    {
        return x * y;
    }
}

/// <summary>
/// A utility class for string operations.
/// </summary>
public static class StringHelper
{
    /// <summary>
    /// Reverses a string.
    /// </summary>
    /// <param name="input">The string to reverse.</param>
    /// <returns>The reversed string.</returns>
    public static string Reverse(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        char[] charArray = input.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    /// <summary>
    /// Converts a string to uppercase.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>The uppercase string.</returns>
    public static string ToUpperCase(string input)
    {
        return input?.ToUpper() ?? string.Empty;
    }

    /// <summary>
    /// Checks if a string is a palindrome.
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <returns>True if the string is a palindrome, false otherwise.</returns>
    public static bool IsPalindrome(string input)
    {
        if (string.IsNullOrEmpty(input))
            return true;

        var cleaned = input.ToLower().Replace(" ", "");
        return cleaned == Reverse(cleaned).ToLower();
    }
}

/// <summary>
/// Represents a person with basic information.
/// </summary>
public class Person
{
    /// <summary>
    /// Gets or sets the first name of the person.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name of the person.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets the full name of the person.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Gets or sets the age of the person.
    /// </summary>
    public int Age { get; set; }

    /// <summary>
    /// Initializes a new instance of the Person class.
    /// </summary>
    public Person() { }

    /// <summary>
    /// Initializes a new instance of the Person class with specified values.
    /// </summary>
    /// <param name="firstName">The first name.</param>
    /// <param name="lastName">The last name.</param>
    /// <param name="age">The age.</param>
    public Person(string firstName, string lastName, int age)
    {
        FirstName = firstName;
        LastName = lastName;
        Age = age;
    }

    /// <summary>
    /// Returns a string representation of the person.
    /// </summary>
    /// <returns>A string containing the person's information.</returns>
    public override string ToString()
    {
        return $"{FullName}, Age: {Age}";
    }
}

/// <summary>
/// Interface for data repository operations.
/// </summary>
/// <typeparam name="T">The type of entity.</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Gets an entity by its ID.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <returns>The entity if found, null otherwise.</returns>
    T? GetById(int id);

    /// <summary>
    /// Gets all entities.
    /// </summary>
    /// <returns>A collection of all entities.</returns>
    IEnumerable<T> GetAll();

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    void Add(T entity);

    /// <summary>
    /// Deletes an entity by its ID.
    /// </summary>
    /// <param name="id">The ID of the entity to delete.</param>
    /// <returns>True if deletion was successful, false otherwise.</returns>
    bool Delete(int id);
}

/// <summary>
/// Enum representing days of the week.
/// </summary>
public enum DayOfWeek
{
    /// <summary>
    /// Sunday
    /// </summary>
    Sunday = 0,
    
    /// <summary>
    /// Monday
    /// </summary>
    Monday = 1,
    
    /// <summary>
    /// Tuesday
    /// </summary>
    Tuesday = 2,
    
    /// <summary>
    /// Wednesday
    /// </summary>
    Wednesday = 3,
    
    /// <summary>
    /// Thursday
    /// </summary>
    Thursday = 4,
    
    /// <summary>
    /// Friday
    /// </summary>
    Friday = 5,
    
    /// <summary>
    /// Saturday
    /// </summary>
    Saturday = 6
}

/// <summary>
/// Struct representing a point in 2D space.
/// </summary>
public struct Point
{
    /// <summary>
    /// Gets or sets the X coordinate.
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate.
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Initializes a new instance of the Point struct.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public Point(double x, double y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Calculates the distance from this point to another point.
    /// </summary>
    /// <param name="other">The other point.</param>
    /// <returns>The distance between the two points.</returns>
    public double DistanceTo(Point other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}

/// <summary>
/// Event arguments for value changed events.
/// </summary>
public class ValueChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the old value.
    /// </summary>
    public int OldValue { get; }

    /// <summary>
    /// Gets the new value.
    /// </summary>
    public int NewValue { get; }

    /// <summary>
    /// Initializes a new instance of the ValueChangedEventArgs class.
    /// </summary>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    public ValueChangedEventArgs(int oldValue, int newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}

/// <summary>
/// A counter class that raises events when its value changes.
/// </summary>
public class Counter
{
    private int _value;

    /// <summary>
    /// Event raised when the counter value changes.
    /// </summary>
    public event EventHandler<ValueChangedEventArgs>? ValueChanged;

    /// <summary>
    /// Gets the current value of the counter.
    /// </summary>
    public int Value
    {
        get => _value;
        set
        {
            var oldValue = _value;
            _value = value;
            OnValueChanged(new ValueChangedEventArgs(oldValue, value));
        }
    }

    /// <summary>
    /// Increments the counter by one.
    /// </summary>
    public void Increment()
    {
        Value++;
    }

    /// <summary>
    /// Decrements the counter by one.
    /// </summary>
    public void Decrement()
    {
        Value--;
    }

    /// <summary>
    /// Raises the ValueChanged event.
    /// </summary>
    /// <param name="e">Event arguments.</param>
    protected virtual void OnValueChanged(ValueChangedEventArgs e)
    {
        ValueChanged?.Invoke(this, e);
    }
}
