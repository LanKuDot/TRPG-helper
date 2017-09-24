using System;
using System.Text;

namespace discordTRPGHelper
{
    class Dice
    {
        private Random _rnd;

        public Dice()
        {
            _rnd = new Random();
        }

        /*
         * @brief Get the dicing result in string.
         * @param formula Specify the dicing formula, such as "4 + 6D6".
         * @return The dicing result in string.
         */
        public string GetDiceResult(string formula)
        {
            StringBuilder outputStr = new StringBuilder();
            int result = 0;

            /* Handle the dice command */
            int[] diceResult = RollTheDice(formula);
            outputStr.Append("=> " + formula + "(");
            foreach (int d in diceResult) {
                result += d;
                outputStr.Append(d.ToString() + "+");
            }
            outputStr[outputStr.Length - 1] = ')'; // Replace the last "+" to ")"
            outputStr.Insert(0, result.ToString() + " ");

            return outputStr.ToString();
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
        private int[] RollTheDice(string diceCmd)
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
