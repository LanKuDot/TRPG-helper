using System;
using System.Text;
using System.Text.RegularExpressions;

namespace discordTRPGHelper
{
    class Dice
    {
        private enum ElementType
        {
            Invaild = 0,
            Number,
            Dice
        };

        private Random _rnd;    // The dice
        private Regex _regexForDice;

        public Dice()
        {
            _rnd = new Random();
            _regexForDice = new Regex(@"^\d*[Dd]\d+$");
        }

        /*
         * @brief Calculate the dicing result from formula and return the result in string.
         *
         * The format of _formula_ is "[+/-]<element>(<+/-><element>)*".
         * The _formula_ is seperated by '+' or '-', and all the substrings are "element".
         * The dice (such as 3D4) and the number (such as 2) are the vaild elements, and
         * invaild elements will generate 0.
         * The method will return a null string if the input _formula_ contains continous operators.
         *
         * @param formula Specify the dicing formula, such as "4 + 6D6".
         * @return The dicing result in string.
         */
        private string Calculate(string formula)
        {
            StringBuilder outputStr = new StringBuilder();
            int finalSum = 0;

            /* Parse the dice formula */
            // Remove all the spaces in the formula
            formula = formula.Replace(" ", string.Empty);

            // Get each element from the formula and do corresponding action
            char[] searchAny = { '+', '-' };
            bool isPlus = true;
            // operatorId always is the index of '+' or '-'.
            // elementFrom and elementTo mark the substring index of the element.
            int operatorId = formula.IndexOfAny(searchAny), elementFrom = 0, elementTo = 0;

            // Insert a leading '+'
            if (operatorId != 0) {
                formula = formula.Insert(0, "+");
            }

            do {
                /* Find the operator */
                operatorId = formula.IndexOfAny(searchAny, elementFrom);
                isPlus = (formula[operatorId] == '+') ? true : false;

                /* Find the element */
                elementFrom = operatorId + 1;
                operatorId = formula.IndexOfAny(searchAny, elementFrom);
                if (operatorId == -1)
                    elementTo = formula.Length - 1;
                else
                    elementTo = operatorId - 1;
                // Invaild: Continous operators
                if (operatorId == elementFrom)
                    return null;

                /* Get the element sum */
                int[] elementResult = GetElementResult(formula.Substring(elementFrom, elementTo - elementFrom + 1));
                int elementSum = 0;
                foreach (int i in elementResult)
                    elementSum += i;

                /* Add the final value of the element to the final result */
                if (isPlus)
                    finalSum += elementSum;
                else
                    finalSum -= elementSum;

                /* Append the message to the output string */
                // Append the operator
                if (elementFrom == 1)    // Append the leading '-' if needed
                    outputStr.Append(isPlus ? "" : "-");
                else
                    outputStr.Append(isPlus ? '+' : '-');

                // Append the element sum
                if (elementResult.Length == 1)
                    outputStr.Append(elementSum.ToString());
                else {  // The multiple dice result
                    outputStr.Append('(');
                    foreach(int i in elementResult)
                        outputStr.Append(i.ToString() + "+");
                    outputStr.Replace('+', ')', outputStr.Length - 1, 1);   // Replace the last '+'
                }
            } while (operatorId != -1);

            /* Insert the final result at the begin of the string */
            outputStr.Insert(0, finalSum.ToString() + " => ");

            return outputStr.ToString();
        }

        /*
         * @brief Get the calculating result from the given element.
         *
         * If the input element is invaild, the method will return an array
         * with only one element '0'.
         *
         * @param element Specify the content of the element.
         * @return An array contains the result of each dice or a number.
         */
        private int[] GetElementResult(string element)
        {
            int[] singleReult = { 0 };

            switch (GetElementType(element)) {
            // The dice command
            case ElementType.Dice:
                int[] diceResult = GetDiceResult(element);

                // The formula is invaild
                if (diceResult == null)
                    break;

                return diceResult;

            // The number
            case ElementType.Number:
                singleReult[0] = int.Parse(element);
                break;

            // Invaild input
            default:
                break;
            }

            return singleReult;
        }

        /*
         * @brief Get the type of the element in the formula
         * @param element Specify the content of the element.
         * @return The type of the element
         */
        private ElementType GetElementType(string element)
        {
			int i;

            if (string.IsNullOrEmpty(element))
                return ElementType.Invaild;
            if (int.TryParse(element, out i))
                return ElementType.Number;
            if (_regexForDice.IsMatch(element))
                return ElementType.Dice;

            return ElementType.Invaild;
        }

        /*
         * @brief Roll the dice.
         *
         * The format of the _diceCmd_ will be "[number\_of\_dices]<d/D><number\_of\_faces>", which
         * the number of dice is 1 when _number\_of\_dices_ is not specified.
         *
         * @param diceCmd Specify the number and the faces of the dices, such as "3d6" or "4D12".
         * @return An array of the dicing result of each dice
         * @retval null If the diceCmd is invaild.
         */
        private int[] GetDiceResult(string diceCmd)
        {
            int dices, faces;

            /* Parse the diceCmd */
            int d = diceCmd.IndexOf("D");
            if (d < 0)  // Not "D", instead of "d".
                d = diceCmd.IndexOf("d");

            if (d == 0) // number_of_dices is not specified
                dices = 1;
            else
                dices = int.Parse(diceCmd.Substring(0, d));

            faces = int.Parse(diceCmd.Substring(d + 1));

            // Check if the value is vaild
            if (dices < 1 || faces < 1)
                return null;

            /* Roll the dice */
            int[] diceResult = new int[dices];
            for (int i = 0; i < dices; ++i)
                diceResult[i] = _rnd.Next(1, faces + 1);

            return diceResult;
        }
    }
}
