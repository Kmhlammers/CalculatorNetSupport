using CalculatorEmpty.Models;
using Microsoft.AspNetCore.Mvc;

namespace CalculatorEmpty.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View(new CalculatorModel { Expression = "", Result = "" });
        }

        [HttpPost]
        public IActionResult Calculate(CalculatorModel model)
        {
                     
            try
            {
                
                if (model.Expression != null)
                {                    
                    model.Result = CalculateEquation(model.Expression).ToString();
                }

            }
            
            catch (DivideByZeroException)
            {
                model.ErrorMessage = "Error: Division by zero";
            }
            catch (OverflowException)
            {
                model.ErrorMessage = "Error: The number you entered was either too large or too small for this calculator";
            }
            catch (FormatException ex)
            {
                model.ErrorMessage = "Error: " + ex.Message;
            }
            catch (InvalidOperationException ex)
            {
                model.ErrorMessage = "Error: " + ex.Message;
            }
            catch (ArgumentException ex)
            {
                model.ErrorMessage = "Error: " + ex.Message;
            }
            catch (IndexOutOfRangeException) 
            {
                model.ErrorMessage = "Error: Index out of range";
            }
            catch
            {                
                model.ErrorMessage = "Error: Invalid input";
            }
            

            return View("Index", model);
        }

        decimal CalculateEquation(string expression)
        {
            /// 
            /// Takes a string as input. If this input is a valid equation
            /// it will return the solution of this equation using correct order of operations PEMDAS:
            /// Otherwise it will display an error.
            ///
            decimal total;
            string trimmedExpression = expression.Trim(); // .Replace(".", ",");
                                                          // !!! Add this line of code above if a dot is
                                                          // seen as a separator instead of a decimal !!! 

            // The expression is evaluated and all characters are added to a list 
            List<string> numbers = IsolateCharacters(trimmedExpression);

            // The equation is calculated in the correct PEMDAS order
            numbers = Brackets(numbers); // Parentheses
            numbers = Exponent(numbers);
            numbers = MultiplyAndDivide(numbers);
            total = AddAndSubstract(numbers);

            return total;
        }

        List<string> IsolateCharacters(string expression)
        {
            ///
            /// This method takes a string as input and adds valid characters 
            /// to a list. If characters are not valid it will throw an error.
            ///


            int previousIndex = 0;

            // Initialize the valid characters 
            string operations = "+-*/^";
            string brackets = "()";
            HashSet<char> validChars = new HashSet<char> { '+', '-', '*', '/', '(', ')', '^', ' ', '.', ',' };

            //Initialize count for brackets to check if there is a mismatch
            int countOpenBrackets = 0;
            int countClosingBrackets = 0;

            List<string> isolatedCharacters = new List<string>(); // list to store characters of equation

            for (int i = 0; i < expression.Length; i++)
            {
                // first check if the character is valid
                if (!char.IsDigit(expression[i]) && !validChars.Contains(expression[i]))
                {
                    throw new FormatException($"Invalid input: {expression[i]} is an invalid character.");
                }

                else if (operations.Contains(expression[i]))
                {
                    bool noNumberBefore = expression.Substring(previousIndex, i - previousIndex).Trim() == "";

                    // if equation begins with "-" only add the "-" if the next character is a "("
                    // otherwise the "-" belongs to a number so do nothing
                    if (i == 0 && expression[i] == '-')
                    {

                        //account for possible white spaces
                        int nextNonWhiteSpace = i + 1;
                        while (nextNonWhiteSpace < expression.Length && char.IsWhiteSpace(expression[nextNonWhiteSpace]))
                        {
                            nextNonWhiteSpace++;
                        }

                        if (expression[nextNonWhiteSpace] == '(')
                        {
                            isolatedCharacters.Add("-");
                        }
                    }
                    // handle the cases: +-, *-, /- and --
                    
                    else if (expression[i] == '-' && isolatedCharacters.Count > 0 && operations.Contains(isolatedCharacters[^1]) && noNumberBefore)
                    {
                        if (isolatedCharacters[^1] == "-")
                        {
                            isolatedCharacters[^1] = "+";
                            previousIndex = i + 1;
                        }
                    }
                    // give an error for the cases: +*, -*, ^*, etc...
                    else if (i > 0 && isolatedCharacters.Count() > 0 && operations.Contains(isolatedCharacters[^1]) && expression[i] != '-' && noNumberBefore)
                    {
                        throw new InvalidOperationException($"Invalid expression: Consecutive operators '{isolatedCharacters[^1]}{expression[i]}' found.");
                    }
                    // the normal case, add number before operator and add the operator.
                    else
                    {
                        string number = expression.Substring(previousIndex, i - previousIndex).Trim();
                        if (number != "")
                        {
                            isolatedCharacters.Add(number);
                        }

                        isolatedCharacters.Add(expression[i].ToString().Trim());
                        previousIndex = i + 1;


                    }

                }
                else if (brackets.Contains(expression[i]))
                {

                    if (countOpenBrackets >= countClosingBrackets) // make sure there is no mismatch in brackets
                    {
                        if (expression[i] == '(')
                        {
                            countOpenBrackets += 1;

                            // Add a multiply sign if a number is before a opening bracket. Ex: 6(3+1)
                            if (i >= 1 && decimal.TryParse(expression.Substring(previousIndex, i - previousIndex).Trim(), out decimal multiplyValue))
                            {
                                isolatedCharacters.Add($"{multiplyValue}");
                                isolatedCharacters.Add("*");
                            }
                            //Add a multiply sign between )(
                            else if (isolatedCharacters.Count > 0 && isolatedCharacters[^1] == ")")
                            {
                                isolatedCharacters.Add("*");
                            }

                            isolatedCharacters.Add(expression[i].ToString().Trim());
                            previousIndex = i + 1;
                        }
                        else if (expression[i] == ')')
                        {
                            countClosingBrackets += 1;
                            string number = expression.Substring(previousIndex, i - previousIndex).Trim();

                            if (number != "")
                            {
                                isolatedCharacters.Add(number);
                            }

                            isolatedCharacters.Add(")");
                            previousIndex = i + 1;

                            int nextNonWhiteSpace = i + 1;
                            while (nextNonWhiteSpace < expression.Length && char.IsWhiteSpace(expression[nextNonWhiteSpace]))
                            {
                                nextNonWhiteSpace++;
                            }

                            // Add multiple sign when number is behind ). Ex: (5+3)4
                            if (nextNonWhiteSpace < expression.Length && char.IsDigit(expression[nextNonWhiteSpace]))
                            {
                                isolatedCharacters.Add("*");
                            }


                        }
                    }
                    else //there are more closing brackets than opening bracktets
                    {
                        throw new InvalidOperationException("Mismatched brackets");
                    }
                }


            }
            // Error when at the end the opening brackets and closing brackets are not equal
            if (countOpenBrackets != countClosingBrackets)
            {
                throw new InvalidOperationException("Mismatched brackets");
            }


            // Add the final number to the list
            if (previousIndex < expression.Length)
            {
                isolatedCharacters.Add(expression.Substring(previousIndex).Trim());
            }

            return isolatedCharacters;
        }

        List<string> Brackets(List<string> items)
        {
            /// 
            /// This function takes a list and returns a list with no brackets and where
            /// the expressions inside the brackets are solved by calling the methods:
            /// Exponent(), MultiplyAndDivide(), and AddAndSubstract(). The items
            /// of the bracketed expression are then replaced with that solution.
            ///

            Stack<int> bracketStack = new Stack<int>(); // Stack to keep track of '(' indices

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == "(")
                {
                    bracketStack.Push(i);
                }
                else if (items[i] == ")")
                {
                    if (bracketStack.Count > 0)
                    {
                        // The last added opening bracket belongs to this closing bracket
                        int indexFirstBracket = bracketStack.Pop();
                        int indexLastBracket = i;


                        List<string> innerSlice = items.GetRange(indexFirstBracket + 1, indexLastBracket - indexFirstBracket - 1);

                        // Calculate the result of the inner expression

                        List<string> calculatedSlice = Exponent(innerSlice);
                        calculatedSlice = MultiplyAndDivide(innerSlice);
                        decimal valueSlice = AddAndSubstract(calculatedSlice);

                        // Replace the entire bracketed expression with the calculated result
                        items[indexFirstBracket] = valueSlice.ToString();
                        items.RemoveRange(indexFirstBracket + 1, indexLastBracket - indexFirstBracket);

                        // To account for the indice shift caused by removing items
                        i = indexFirstBracket;
                    }
                }
            }

            return items;
        }

        List<string> Exponent(List<string> items)
        {
            ///
            /// Function that takes a list and returns a list where all
            /// exponent expressions are solved.
            ///

            for (int i = 0; i < items.Count; i++)
            {
                if (i == 0 && items[i] == "^") // expression can not start with "^"
                {
                    throw new ArgumentException($"Invalid expression: cannot start with '{items[0]}'");
                }

                else if (items[i] == "^")
                {
                    if (i + 1 < items.Count())
                    {
                        bool isValidBaseValue = decimal.TryParse(items[i - 1], out decimal baseValue);
                        bool isValidExponent = decimal.TryParse(items[i + 1], out decimal exponent);

                        if (isValidBaseValue && isValidExponent)
                        {
                            decimal result = (decimal)Math.Pow((double)baseValue, (double)exponent);

                            // Replace the exponent expression with the result
                            items[i - 1] = result.ToString();
                            items.RemoveRange(i, 2);

                            // Set i to acccount for the index shift
                            i -= 2;

                        }
                        else // one or both of the two values is not a decimal
                        {
                            throw new InvalidOperationException("Invalid input");
                        }
                    }
                    else
                    {
                        throw new FormatException("Invalid format: expression is not complete");
                    }
                }
            }
            return items;
        }

        List<string> MultiplyAndDivide(List<string> items)
        {
            ///
            /// This function takes a list and returns a list where all multiplication and
            /// division expressions are solved
            ///

            bool isValidNumber1 = false;
            bool isValidNumber2 = false;
            bool hasMultiplicationOrDivision = true;

            do
            {
                int index = items.FindIndex(op => op == "*" || op == "/");

                if (index != -1)
                {
                    decimal calculatedValue;

                    if (index == 0) // case where equation starts with * or /
                    {
                        throw new ArgumentException($"Invalid expression: cannot start with '{items[0]}'");
                    }

                    isValidNumber1 = decimal.TryParse(items[index - 1], out decimal value1);
                    isValidNumber2 = decimal.TryParse(items[index + 1], out decimal value2);

                    if (isValidNumber1 && isValidNumber2) // check if the numbers are valid
                    {
                        if (items[index] == "*")
                        {
                            calculatedValue = value1 * value2;
                        }
                        else
                        {
                            if (value2 == 0) //Division by zero is not possible
                            {
                                throw new DivideByZeroException();
                            }
                            calculatedValue = value1 / value2;
                        }

                        // Replace the calculated expression with the result
                        items.RemoveAt(index + 1);
                        items.RemoveAt(index);
                        items[index - 1] = $"{calculatedValue}";
                    }
                    else // one of the two numbers is not a decimal
                    {
                        throw new InvalidOperationException("Invalid input");
                    }

                }
                else
                    hasMultiplicationOrDivision = false;

            } while (hasMultiplicationOrDivision);

            return items;
        }

        decimal AddAndSubstract(List<string> items)
        {
            ///
            /// This function takes a list and returns a decimal value
            /// when this lists consists of valid addition and substraction
            /// expressions.
            ///

            decimal.TryParse(items[0], out decimal totalValue);
            bool isValidNumber = false;

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == "+" || items[i] == "-")
                {
                    if (i + 1 >= items.Count) // make sure there is a number to calculate
                    {
                        throw new ArgumentException("Invalid expression");
                    }
                    else if (items[i] == "+")
                    {
                        isValidNumber = decimal.TryParse(items[i + 1], out decimal value2);

                        totalValue += value2;
                    }
                    else if (items[i] == "-")
                    {
                        if (items[i + 1] == "-" && i + 2 < items.Count) //case --
                        {
                            items.RemoveAt(i + 1); // remove the second minus
                            isValidNumber = decimal.TryParse(items[i + 1], out decimal value2);

                            //instead of substracting value2 we add it because of the double minus
                            totalValue += value2;
                        }
                        else
                        {
                            isValidNumber = decimal.TryParse(items[i + 1], out decimal value2);

                            totalValue -= value2;
                        }

                    }
                    if (!isValidNumber) // check if the number parsed is valid
                    {
                        throw new ArgumentException("Invalid expression");
                    }
                }
            }

            return totalValue;
        }
    }
}
