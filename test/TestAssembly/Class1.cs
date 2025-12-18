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
}
