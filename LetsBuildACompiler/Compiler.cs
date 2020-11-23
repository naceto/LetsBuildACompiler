using System;

namespace LetsBuildACompiler
{
    public class Compiler
    {
        private const string Tab = "\t";
        private const char CR = '\r';

        private char look;

        public void Compile()
        {
            Init();
            Assignment();
            if (look != CR) 
            {
                Expected("Newline");
            }
        }

        private void GetChar()
        {
            look = (char)Console.Read();
        }

        private static void Error(string error)
        {
            Console.WriteLine();
            Console.WriteLine($"Error: {error}.");
        }

        private static void Abort(string error)
        {
            Error(error);
            Environment.Exit(-1);
        }

        private static void Expected(string expected)
        {
            Abort($"{expected} Expected");
        }

        private void Match(char c)
        {
            if (look == c)
            {
                GetChar();
            }
            else
            {
                Expected($"'''' + {c} + ''''");
            }
        }

        private static bool IsAlpha(char c)
        {
            return char.IsLetter(c);
        }

        private static bool IsDigit(char c)
        {
            return char.IsDigit(c);
        }

        private char GetName()
        {
            if (!IsAlpha(look))
            {
                Expected("Name");
            }

            char rv = char.ToUpper(look);
            GetChar();
            return rv;
        }

        private char GetNum()
        {
            if (!IsDigit(look))
            {
                Expected("Integer");
            }

            char rv = look;
            GetChar();
            return rv;
        }

        private static void Emit(string s, string end = "")
        {
            Console.Write(Tab + s + end);
        }

        private static void EmitLn(string s)
        {
            Emit(s, "\n");
        }

        private void Init()
        {
            GetChar();
        }

        // Parse and Translate an Identifier
        private void Indent()
        {
            var name = GetName();
            if (look == '(')
            {
                Match('(');
                Match(')');
                EmitLn("BSR " + name);
            }
            else
            {
                EmitLn("MOVE " + name + "(PC),D0");
            }
        }

        private void Factor()
        {
            if (look == '(')
            {
                Match('(');
                Expression();
                Match(')');
            }
            else if(IsAlpha(look))
            {
                Indent();
            }
            else
            {
                EmitLn("MOVE #" + GetNum() + ",D0");
            }
        }

        private void Add()
        {
            Match('+');
            Term();
            EmitLn("ADD (SP)+,D0");
        }

        private void Subtract()
        {
            Match('-');
            Term();
            EmitLn("SUB (SP)+,D0");
            EmitLn("NEG D0");
        }

        private void Multiply()
        {
            Match('*');
            Factor();
            EmitLn("MULS (SP)+,D0");
        }

        private void Divide()
        {
            Match('/');
            Factor();
            EmitLn("MOVE (SP)+,D1");
            EmitLn("DIVS D1,D0");
        }

        private void Term()
        {
            Factor();
            while (look == '*' || look == '/')
            {
                EmitLn("MOVE D0,-(SP)");
                switch(look)
                {
                    case '*':
                        Multiply();
                        break;
                    case '/':
                        Divide();
                        break;
                    default:
                        //Expected("Multop");
                        break;
                }
            }
        }

        private bool IsAddop(char c)
        {
            return c == '+' || c == '-';
        }

        private void Expression()
        {
            if (IsAddop(look))
            {
                EmitLn("CLR D0");
            }
            else
            {
                Term();
            }

            while (look == '+' || look == '-')
            {
                EmitLn("MOVE D0,-(SP)");
                switch (look)
                {   
                    case '+':
                        Add();
                        break;
                    case '-':
                        Subtract();
                        break;
                    default:
                        //Expected("Addop");
                        break;
                }
            }
        }

        private void Assignment()
        {
            var name = GetName();
            Match('=');
            Expression();
            EmitLn("LEA " + name + "(PC),A0");
            EmitLn("MOVE D0,(A0)");
        }
    }
}
