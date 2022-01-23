using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.IO;

namespace Ultimate_Triclustering.Parsers
{
    public abstract class BaseParser
    {
        protected enum LexType
        {
            LEX_NULL,
            LEX_DELIM,
            LEX_DOMAIN,
            LEX_NUMBER,
            LEX_NAME,
            LEX_EOF
        };

        protected enum LexDelims
        {
            LEX_DEL_NULL,
            LEX_DEL_POW,
            LEX_DEL_LESS,
            LEX_DEL_LEQ,
            LEX_DEL_EQ,
            LEX_DEL_GR,
            LEX_DEL_GREQ,
            LEX_DEL_NEQ,
            LEX_DEL_MULT,
            LEX_DEL_DIV,
            LEX_DEL_ADD,
            LEX_DEL_SUB,
            LEX_DEL_UN,
            LEX_DEL_INT,
            LEX_DEL_COM,
            LEX_DEL_POPEN,
            LEX_DEL_PCLOSE,
            LEX_DEL_ABS,
            LEX_DEL_COL,
            LEX_DEL_CAR
        };

        protected List<string> LEX_DELIMS = new List<string>() {
            "",
            "^",
            "<",
            "<=",
            "=",
            ">",
            ">=",
            "!=",
            "*",
            "/",
            "+",
            "-",
            "U",
            "/\\",
            "\\",
            "(",
            ")",
            "|",
            ":",
            "#"
        };

        protected enum LexNames
        {
            LEX_NAM_NULL,
            LEX_NAM_SUM,
            LEX_NAM_REL,
            LEX_NAM_CON,
            LEX_NAM_DOM
        };

        protected List<string> LEX_NAMES = new List<string>() {
            "",
            "sum",
            "R",
            "C",
            "D"
        };

        protected string expression;
        protected char curChar;

        protected void gc()
        {
            expression = expression.Remove(0, 1);
            curChar = expression[0];
        }

        /* Lexeme grammar
         * S -> ' 'S | 'digit'N | 'letter'C | = | != | ^ | < | <= | > | >= | * | / | + | - | \/ | /\ | \ | '|' | ( | ) | | # | '\0'
         * N -> 'digit'N | .F | eps
         * F -> 'digit'F | 'digit'
         * C -> 'letter'C | eps
        */

        enum State { S, N, F, C };

        protected Lexeme ReadNextLexeme()
        {
            State curState = State.S;

            string buf = "";

            while (true)
            {
                switch (curState)
                {
                    case State.S:
                        if (curChar == ' ')
                        {
                            gc();
                            curState = State.S;
                        }
                        else if (char.IsDigit(curChar))
                        {
                            buf += curChar;
                            gc();
                            curState = State.N;
                        }
                        else if (char.IsLetter(curChar))
                        {
                            buf += curChar;
                            gc();
                            curState = State.C;
                        }
                        else if (curChar == '\0')
                        {
                            return new Lexeme((int)LexType.LEX_EOF, (int)LexType.LEX_NULL, "\0", 0.0);
                        }
                        else
                        {
                            buf += curChar;
                            gc();

                            if (buf == "!" && curChar == '=')
                            {
                                buf += curChar;
                                gc();
                            }

                            if (buf == "<" || buf == ">")
                            {
                                if (curChar == '=')
                                {
                                    buf += curChar;
                                    gc();
                                }
                            }

                            if (buf == "/" && curChar == '\\')
                            {
                                buf += curChar;
                                gc();
                            }

                            if (buf == "\\" && curChar == '/')
                            {
                                buf += curChar;
                                gc();
                            }

                            int index = LEX_DELIMS.IndexOf(buf);

                            if (index > 0)
                                return new Lexeme((int)LexType.LEX_DELIM, index, buf, 0.0);
                            else
                                return new Lexeme((int)LexType.LEX_NULL, (int)LexType.LEX_NULL, buf, 0.0);
                        }
                        break;
                    case State.N:
                        if (char.IsDigit(curChar))
                        {
                            buf += curChar;
                            gc();
                            curState = State.N;
                        }
                        else if (curChar == '.')
                        {
                            buf += curChar;
                            gc();
                            curState = State.F;
                        }
                        else
                        {
                            return new Lexeme((int)LexType.LEX_NUMBER, (int)LexType.LEX_NULL, buf, Convert.ToDouble(buf));
                        }
                        break;
                    case State.C:
                        if (char.IsLetter(curChar))
                        {
                            buf += curChar;
                            gc();
                            curState = State.C;
                        }
                        else
                        {
                            int index = LEX_NAMES.IndexOf(buf);
                            if (index > 0)
                                return new Lexeme((int)LexType.LEX_NAME, index, buf, 0.0);
                            else
                            {
                                return new Lexeme((int)LexType.LEX_DOMAIN, (int)LexType.LEX_NULL, buf, 0.0);
                            }
                        }
                        break;
                }
            }
        }

        protected Lexeme curLex;

        protected void GetNextLexeme()
        {
            curLex = ReadNextLexeme();
        }
    }
}