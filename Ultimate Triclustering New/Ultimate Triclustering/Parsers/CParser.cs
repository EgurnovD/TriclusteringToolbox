using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.IO;
using System.Linq;

namespace Ultimate_Triclustering.Parsers
{
    public class CParser : BaseParser
    {
        /* Grammar
         * S -> H/\S | H'\0'
         * H -> E<E | E<=E | E=E | E>=E | E>E
         * E -> -L | L
         * L -> P+L | P-L | P
         * P -> F*P | (E)*P | F*(E) | (E)*(E) | F/P | (E)/P | F/(E) | (E)/(E) | (E) | '|'E'|' | F
         * F -> T | T^P
         * T -> #(K) | N                       ######## 'sum'(K) | 
         * K -> M | M \/ K | 'R'\'C'+ | 'R'\'C'-           ########C(nn:nn) | C(nn:z:nn)    // nn - natural number, z - integer
         * M -> C/\M | (K)/\M | C/\(K) | (K)/\(K) | (K)
         * C -> 'D'\Set | Set
         * N -> nn | z | r
         * Set -> nn'+' | nn'-'
        */

        Tricluster U;
        Tricluster V;
        TriadicContext context;

        public CParser()
        {
        }

        public KeyValuePair<bool,string> CalcConstraint(string constraint, Tricluster U, Tricluster V, TriadicContext cont)
        {
            this.U = U;
            this.V = V;
            this.expression = constraint + "\0";
            context = cont;

            //string tempExpr = constraint.ToString();
            bool result;

            try
            {
                curChar = this.expression[0];
                GetNextLexeme();
                result = parseS();
            }
            catch (ParseException exc)
            {
                return new KeyValuePair<bool, string>(false, exc.comment);
            }
            
            if (this.expression == "\0")
            {
                return new KeyValuePair<bool,string>(result, null);
            }
            else
            {
                return new KeyValuePair<bool, string>(false, "Incorect expression");
            }
        }

        /*private double CalcRPN(string expr)
        {
            List<string> procExpr = new List<string>();
            Stack<string> stack = new Stack<string>();
            string curStr = expr[0].ToString();
            expr = expr.Substring(1);
            string temp = "";
            while (true)
            {
                if (curStr == " ") { }
                else if (curStr == "(" || curStr == "=")
                {
                    stack.Push(curStr);
                }
                else if (curStr == "^" || curStr == "<" || curStr == ">")
                {
                    while (stack.Count != 0 && stack.Peek() == "=")
                    {
                        procExpr.Add(stack.Pop());
                    }
                    stack.Push(curStr);
                }
                else if (curStr == "&")
                {
                    if (stack.Count != 0)
                    {
                        temp = stack.Peek();
                        while (temp == "=" || temp == "^" || temp == "<" || temp == ">" || temp == "&")
                        {
                            procExpr.Add(stack.Pop());
                            if (stack.Count == 0)
                            {
                                break;
                            }
                            temp = stack.Peek();
                        }
                    }
                    stack.Push(curStr);
                }
                else if (curStr == "|")
                {
                    if (stack.Count != 0)
                    {
                        temp = stack.Peek();
                        while (temp == "=" || temp == "^" || temp == "<" || temp == ">" || temp == "&" || temp == "|")
                        {
                            procExpr.Add(stack.Pop());
                            if (stack.Count == 0)
                            {
                                break;
                            }
                            temp = stack.Peek();
                        }
                    }
                    stack.Push(curStr);
                }
                else if (curStr == "+")
                {
                    if (stack.Count != 0)
                    {
                        temp = stack.Peek();
                        while (temp == "=" || temp == "^" || temp == "<" || temp == ">" || temp == "&" || temp == "|" || temp == "+")
                        {
                            procExpr.Add(stack.Pop());
                            if (stack.Count == 0)
                            {
                                break;
                            }
                            temp = stack.Peek();
                        }
                    }
                    stack.Push(curStr);
                }
                else if (curStr == "\"")
                {
                    temp = curStr + expr.Substring(0, expr.IndexOf("\"") + 1);
                    expr = expr.Substring(expr.IndexOf("\"") + 1);
                    procExpr.Add(temp);
                }
                else if (curStr == ")")
                {
                    while (stack.Count != 0 && stack.Peek() != "(")
                    {
                        procExpr.Add(stack.Pop());
                    }
                    
                    //if (stack.Count == 0)
                    //{
                    //    throw new ParseException("Unbalanced parentheses!");
                    //}
                    
                    stack.Pop();
                }
                else
                {
                    int i;
                    List<char> temList = new List<char>() { '=', '^', '<', '>', '&', '|', '+', '(', ')', '\"' };

                    for (i = 0; i < expr.Length; i++)
                    {
                        if (temList.Contains(expr[i]))
                        {
                            break;
                        }
                    }
                    temp = curStr + expr.Substring(0, i);
                    expr = expr.Substring(i);
                    procExpr.Add(temp);
                }
                if (expr == "")
                {
                    break;
                }
                curStr = expr[0].ToString();
                expr = expr.Substring(1);
            }
            while (stack.Count != 0)
            {
                procExpr.Add(stack.Pop());
            }
            return procExpr;
        }*/

        private bool parseS()
        {
            bool op1 = parseH();

            if (curLex.type == (int)LexType.LEX_DELIM && curLex.index == (int)LexDelims.LEX_DEL_INT)
            {
                GetNextLexeme();
                return op1 && parseS();
            }
            else if (curLex.type == (int)LexType.LEX_EOF)
            {
                return op1;
            }
            else
            {
                throw new ParseException("Incorrect expression");
            }
        }

        private bool parseH() // S -> E<E'\0' | E<=E'\0' | E=E'\0' | E>=E'\0' | E>E'\0' 
        {
            double op1 = parseE();
            bool result = false;
            if (curLex.type == (int)LexType.LEX_DELIM && (curLex.index == (int)LexDelims.LEX_DEL_LESS || curLex.index == (int)LexDelims.LEX_DEL_LEQ || curLex.index == (int)LexDelims.LEX_DEL_GR || curLex.index == (int)LexDelims.LEX_DEL_GREQ || curLex.index == (int)LexDelims.LEX_DEL_EQ))
            {
                switch (curLex.index)
                {
                    case (int)LexDelims.LEX_DEL_LESS:
                        GetNextLexeme();
                        result = op1 < parseE();
                        break;
                    case (int)LexDelims.LEX_DEL_LEQ:
                        GetNextLexeme();
                        result = op1 <= parseE();
                        break;
                    case (int)LexDelims.LEX_DEL_GREQ:
                        GetNextLexeme();
                        result = op1 >= parseE();
                        break;
                    case (int)LexDelims.LEX_DEL_GR:
                        GetNextLexeme();
                        result = op1 > parseE();
                        break;
                    case (int)LexDelims.LEX_DEL_EQ:
                        GetNextLexeme();
                        result = op1 == parseE();
                        break;
                }

                //GetNextLexeme();
                //parseE();
            }
            else
            {
                throw new ParseException("Comparison operator expected");
            }

            return result;
        }

        private double parseE() // E -> -L | L
        {
            if (curLex.type == (int)LexType.LEX_DELIM && curLex.index == (int)LexDelims.LEX_DEL_SUB)
            {
                GetNextLexeme();
                return parseL(true);
            }
            else
            {
                return parseL();
            }
        }

        private double parseL(bool sub = false) // L -> P+L | P-L | P
        {
            double op1 = parseP();
            if(sub)
            {
                op1 = - op1;
            }
            if (curLex.type == (int)LexType.LEX_DELIM && (curLex.index == (int)LexDelims.LEX_DEL_ADD || curLex.index == (int)LexDelims.LEX_DEL_SUB))
            {
                switch (curLex.index)
                {
                    case (int)LexDelims.LEX_DEL_ADD:
                        GetNextLexeme();
                        return op1 + parseL();
                    case (int)LexDelims.LEX_DEL_SUB:
                        GetNextLexeme();
                        return op1 + parseL(true);
                }
                //GetNextLexeme();
                //parseL();
            }
            return op1;
        }

        private double parseP(bool div = false) // P -> F*P | (E)*P | '|'E'|'*P | F/P | (E)/P | '|'E'|'/P | (E) | '|'E'|' | F
        {
            double op1;
            if (curLex.type == (int)LexType.LEX_DELIM && curLex.index == (int)LexDelims.LEX_DEL_POPEN)
            {
                GetNextLexeme();
                op1 = parseE();
                if(div)
                {
                    op1 = 1 / op1;
                }
                if (curLex.type != (int)LexType.LEX_DELIM || curLex.index != (int)LexDelims.LEX_DEL_PCLOSE)
                {
                    throw new ParseException("')' expected");
                }
                GetNextLexeme();
                if (curLex.type == (int)LexType.LEX_DELIM && (curLex.index == (int)LexDelims.LEX_DEL_MULT || curLex.index == (int)LexDelims.LEX_DEL_DIV))
                {
                    switch (curLex.index)
                    {
                        case (int)LexDelims.LEX_DEL_MULT:
                            GetNextLexeme();
                            return op1 * parseP();
                        case (int)LexDelims.LEX_DEL_DIV:
                            GetNextLexeme();
                            return op1 * parseP(true);
                    }
                    //GetNextLexeme();
                    //parseP();
                }
            }
            else if (curLex.type == (int)LexType.LEX_DELIM && curLex.index == (int)LexDelims.LEX_DEL_ABS)
            {
                GetNextLexeme();
                op1 = Math.Abs(parseE());
                if(div)
                {
                    op1 = 1 / op1;
                }
                if (curLex.type != (int)LexType.LEX_DELIM || curLex.index != (int)LexDelims.LEX_DEL_ABS)
                {
                    throw new ParseException("'|' expected");
                }
                GetNextLexeme();
                if (curLex.type == (int)LexType.LEX_DELIM && (curLex.index == (int)LexDelims.LEX_DEL_MULT || curLex.index == (int)LexDelims.LEX_DEL_DIV))
                {
                    switch(curLex.index)
                    {
                        case (int)LexDelims.LEX_DEL_MULT:
                            GetNextLexeme();
                            return op1 * parseP();
                        case (int)LexDelims.LEX_DEL_DIV:
                            GetNextLexeme();
                            return op1 * parseP(true);
                    }
                    //GetNextLexeme();
                    //parseP();
                }
            }
            op1 = parseF();
            if (curLex.type == (int)LexType.LEX_DELIM && (curLex.index == (int)LexDelims.LEX_DEL_MULT || curLex.index == (int)LexDelims.LEX_DEL_DIV))
            {
                switch(curLex.index)
                {
                        case (int)LexDelims.LEX_DEL_MULT:
                            GetNextLexeme();
                            return op1 * parseP();
                        case (int)LexDelims.LEX_DEL_DIV:
                            GetNextLexeme();
                            return op1 * parseP(true);
                }
                //GetNextLexeme();
                //parseP();
            }
            return op1;
        }

        private double parseF() // F -> T | T^P
        {
            double op1 = parseT();
            if (curLex.type == (int)LexType.LEX_DELIM && curLex.index == (int)LexDelims.LEX_DEL_POW)
            {
                GetNextLexeme();
                return Math.Pow(op1, parseP());
            }
            return op1;
        }

        private double parseT() // T -> #(K) | N
        {
            List<string> op;
            if (curLex.type == (int)LexType.LEX_DELIM && curLex.index == (int)LexDelims.LEX_DEL_CAR)
            {
                GetNextLexeme();
                if (curLex.type == (int)LexType.LEX_DELIM && curLex.index == (int)LexDelims.LEX_DEL_POPEN)
                {
                    GetNextLexeme();
                    op = parseK();
                    if (curLex.type != (int)LexType.LEX_DELIM || curLex.index != (int)LexDelims.LEX_DEL_PCLOSE)
                    {
                        throw new ParseException("')' expected");
                    }
                    GetNextLexeme();
                    return op.Count;
                }
                else
                {
                    throw new ParseException("'(' expected");
                }
            }
            else
            {
                return parseN();
            }
        }

        private List<string> parseK() // K -> M | M \/ K | 'R'\'C'+ | 'R'\'C'-   // ################ Set -> C ///////| C(nn:nn) | C(nn:z:nn)    // nn - natural number, z - integer
        {
            if (curLex.type == (int)LexType.LEX_NAME && curLex.index == (int)LexNames.LEX_NAM_REL)
            {
                bool isMon;
                GetNextLexeme();
                if (curLex.type != (int)LexType.LEX_DELIM || curLex.index != (int)LexDelims.LEX_DEL_COM)
                {
                    throw new ParseException("'\\' expected");
                }
                GetNextLexeme();
                if (curLex.type != (int)LexType.LEX_NAME || curLex.index != (int)LexNames.LEX_NAM_CON)
                {
                    throw new ParseException("'C' expected");
                }
                GetNextLexeme();
                if (curLex.type == (int)LexType.LEX_DELIM && (curLex.index == (int)LexDelims.LEX_DEL_ADD || curLex.index == (int)LexDelims.LEX_DEL_SUB))
                {
                    switch (curLex.index)
                    {
                        case (int)LexDelims.LEX_DEL_ADD:
                            isMon = true;
                            break;
                        case (int)LexDelims.LEX_DEL_SUB:
                            isMon = false;
                            break;
                        default:
                            throw new ParseException("'+' or '-' expected");
                    }
                }
                else
                {
                    throw new ParseException("'+' or '-' expected");
                }
                GetNextLexeme();

                Tricluster cluster = new Tricluster();
                if (isMon)
                {
                    cluster = U.Union(V);
                }
                else
                {
                    cluster = U.Copy();
                }

                List<string> set = new List<string>();
                foreach (string o in context.Objects)
                {
                    foreach (string a in context.Attributes)
                    {
                        foreach (string c in context.Conditions)
                        {
                            if (context.Contains(o, a, c) && (!cluster.extent.Contains(o) || !cluster.intent.Contains(a) || !cluster.modus.Contains(c)))
                            {
                                set.Add("");
                            }
                        }
                    }
                }

                return set;
            }
            else
            {
                List<string> op = parseM();
                if (curLex.type == (int)LexType.LEX_DELIM && curLex.index == (int)LexDelims.LEX_DEL_UN)
                {
                    GetNextLexeme();
                    op = (op.Union(parseK())).ToList<string>();
                }

                return op;
            }
            /*if (curLex.type != (int)LexType.LEX_DOMAIN)
            {
                throw new ParseException("Domain name expected");
            }
            else
            {
                int num = 0;
                for (int i = 0; i < curLex.buf.Length; i++)
                {
                    num += ((int)(Convert.ToChar((curLex.buf[i].ToString()).ToLower())) - (int)('a')) * (int)Math.Pow(26.0, (double)curLex.buf.Length - (double)i - 1.0);
                }
                if (num > context.DomainNum)
                {
                    throw new ParseException("Domain not found");
                }
                else
                {
                    GetNextLexeme();
                    return context.DomainCount(num);
                }
            }*/
            //GetNextLexeme();

            /*if (curLex.type == (int)LexType.LEX_DELIM && curLex.index == (int)LexDelims.LEX_DEL_POPEN)
            {
                GetNextLexeme();
                if (curLex.type != (int)LexType.LEX_NUMBER || curLex.value <= 0 || (curLex.value % 1) != 0)
                {
                    throw new ParseException("Natural number expected");
                }
                GetNextLexeme();
                if (curLex.type != (int)LexType.LEX_DELIM || curLex.index != (int)LexDelims.LEX_DEL_COL)
                {
                    throw new ParseException("Colon expected");
                }
                GetNextLexeme();
                if (curLex.type != (int)LexType.LEX_NUMBER || (curLex.value % 1) != 0 || curLex.value == 0)
                {
                    throw new ParseException("Non-zero integer or natural number expected");
                }
                if (curLex.value < 0)
                {
                    GetNextLexeme();
                    if (curLex.type != (int)LexType.LEX_DELIM || curLex.index != (int)LexDelims.LEX_DEL_COL)
                    {
                        throw new ParseException("Colon expected");
                    }
                    GetNextLexeme();
                    if (curLex.type != (int)LexType.LEX_NUMBER || curLex.value <= 0 || (curLex.value % 1) != 0)
                    {
                        throw new ParseException("Natural number expected");
                    }
                    GetNextLexeme();
                    if (curLex.type != (int)LexType.LEX_DELIM || curLex.index != (int)LexDelims.LEX_DEL_PCLOSE)
                    {
                        throw new ParseException("')' expected");
                    }
                    GetNextLexeme();
                }
                else
                {
                    GetNextLexeme();
                    if (curLex.type == (int)LexType.LEX_DELIM && curLex.index == (int)LexDelims.LEX_DEL_PCLOSE)
                    {
                        GetNextLexeme();
                    }
                    else if (curLex.type == (int)LexType.LEX_DELIM && curLex.index == (int)LexDelims.LEX_DEL_COL)
                    {
                        GetNextLexeme();
                        if (curLex.type != (int)LexType.LEX_NUMBER || curLex.value <= 0 || (curLex.value % 1) != 0)
                        {
                            throw new ParseException("Natural number expected");
                        }
                        GetNextLexeme();
                        if (curLex.type != (int)LexType.LEX_DELIM || curLex.index != (int)LexDelims.LEX_DEL_PCLOSE)
                        {
                            throw new ParseException("')' expected");
                        }
                        GetNextLexeme();
                    }
                    else
                    {
                        throw new ParseException("')' or ':' expected");
                    }
                }
            }*/
        }

        private List<string> parseM() // M -> C/\M | (K)/\M | C/\(K) | (K)/\(K) | (K)
        {
            List<string> op1;
            if (curLex.type == (int)LexType.LEX_DELIM && curLex.index == (int)LexDelims.LEX_DEL_POPEN)
            {
                GetNextLexeme();
                op1 = parseK();

                if (curLex.type != (int)LexType.LEX_DELIM || curLex.index != (int)LexDelims.LEX_DEL_PCLOSE)
                {
                    throw new ParseException("')' expected");
                }
                GetNextLexeme();
                if (curLex.type == (int)LexType.LEX_DELIM && (curLex.index == (int)LexDelims.LEX_DEL_INT || curLex.index == (int)LexDelims.LEX_DEL_COM))
                {
                    switch (curLex.index)
                    {
                        case (int)LexDelims.LEX_DEL_INT:
                            GetNextLexeme();
                            if (curLex.type == (int)LexType.LEX_DELIM && curLex.index == (int)LexDelims.LEX_DEL_POPEN)
                            {
                                GetNextLexeme();
                                List<string> op2 = parseK();
                                if (curLex.type != (int)LexType.LEX_DELIM || curLex.index != (int)LexDelims.LEX_DEL_PCLOSE)
                                {
                                    throw new ParseException("')' expected");
                                }
                                GetNextLexeme();
                                return (op1.Intersect(op2)).ToList<string>();
                            }
                            else
                            {
                                return (op1.Intersect(parseM())).ToList<string>();
                            }
                        case (int)LexDelims.LEX_DEL_UN:
                            GetNextLexeme();
                            if (curLex.type == (int)LexType.LEX_DELIM && curLex.index == (int)LexDelims.LEX_DEL_POPEN)
                            {
                                GetNextLexeme();
                                List<string> op2 = parseK();
                                if (curLex.type != (int)LexType.LEX_DELIM || curLex.index != (int)LexDelims.LEX_DEL_PCLOSE)
                                {
                                    throw new ParseException("')' expected");
                                }
                                GetNextLexeme();
                                return (op1.Union(op2)).ToList<string>();
                            }
                            else
                            {
                                return (op1.Union(parseM())).ToList<string>();
                            }
                        default:
                            throw new ParseException("o_O");
                    }
                }
                else
                {
                    return op1;
                }
            }
            else
            {
                op1 = parseC();
                if (curLex.type == (int)LexType.LEX_DELIM && (curLex.index == (int)LexDelims.LEX_DEL_INT || curLex.index == (int)LexDelims.LEX_DEL_COM))
                {
                    switch (curLex.index)
                    {
                        case (int)LexDelims.LEX_DEL_INT:
                            GetNextLexeme();
                            if (curLex.type == (int)LexType.LEX_DELIM && curLex.index == (int)LexDelims.LEX_DEL_POPEN)
                            {
                                GetNextLexeme();
                                List<string> op2 = parseK();
                                if (curLex.type != (int)LexType.LEX_DELIM || curLex.index != (int)LexDelims.LEX_DEL_PCLOSE)
                                {
                                    throw new ParseException("')' expected");
                                }
                                GetNextLexeme();
                                return (op1.Intersect(op2)).ToList<string>();
                            }
                            else
                            {
                                return (op1.Intersect(parseM())).ToList<string>();
                            }
                        case (int)LexDelims.LEX_DEL_UN:
                            GetNextLexeme();
                            if (curLex.type == (int)LexType.LEX_DELIM && curLex.index == (int)LexDelims.LEX_DEL_POPEN)
                            {
                                GetNextLexeme();
                                List<string> op2 = parseK();
                                if (curLex.type != (int)LexType.LEX_DELIM || curLex.index != (int)LexDelims.LEX_DEL_PCLOSE)
                                {
                                    throw new ParseException("')' expected");
                                }
                                GetNextLexeme();
                                return (op1.Union(op2)).ToList<string>();
                            }
                            else
                            {
                                return (op1.Union(parseM())).ToList<string>();
                            }
                        default:
                            throw new ParseException("o_O");
                    }
                }
                else
                {
                    return op1;
                }
            }
        }


        private List<string> parseC() // C -> 'D'\Set | Set
        {
            if (curLex.type == (int)LexType.LEX_NAME && curLex.index == (int)LexNames.LEX_NAM_DOM)
            {
                GetNextLexeme();
                if (curLex.type != (int)LexType.LEX_DELIM || curLex.index != (int)LexDelims.LEX_DEL_COM)
                {
                    throw new ParseException("'\\' expected");
                }
                GetNextLexeme();
                return parseSet(true);
            }
            else
            {
                return parseSet(false);
            }
        }

        private double parseN() // N -> nn | z | r
        {
            if (curLex.type != (int)LexType.LEX_NUMBER)
            {
                throw new ParseException("Number expected");
            }
            double op = curLex.value;
            GetNextLexeme();
            return op;

        }

        private List<string> parseSet(bool isCom) // Set -> nn
        {
            double op = parseN();
            if(op % 1 != 0 || op < 0)
            {
                throw new ParseException("Natural number expected");
            }
            int cat = Convert.ToInt32(op);
            if (curLex.type == (int)LexType.LEX_DELIM && curLex.index == (int)LexDelims.LEX_DEL_ADD)
            {
                GetNextLexeme();
                if (!isCom)
                {
                    switch (cat)
                    {
                        case 0:
                            return U.extent.Union(V.extent).ToList<string>();
                        case 1:
                            return U.intent.Union(V.intent).ToList<string>();
                        case 2:
                            return U.modus.Union(V.modus).ToList<string>();
                        default:
                            throw new ParseException("Error!");
                    }
                }
                else
                {
                    switch (cat)
                    {
                        case 0:
                            return (context.Objects.Except(U.extent.Union(V.extent))).ToList<string>();
                        case 1:
                            return (context.Attributes.Except(U.intent.Union(V.intent))).ToList<string>();
                        case 2:
                            return (context.Conditions.Except(U.modus.Union(V.modus))).ToList<string>();
                        default:
                            throw new ParseException("Error!");
                    }
                }
            }
            else if (curLex.type == (int)LexType.LEX_DELIM && curLex.index == (int)LexDelims.LEX_DEL_SUB)
            {
                GetNextLexeme();
                if (!isCom)
                {
                    switch (cat)
                    {
                        case 0:
                            return U.extent;
                        case 1:
                            return U.intent;
                        case 2:
                            return U.modus;
                        default:
                            throw new ParseException("Error!");
                    }
                }
                else
                {
                    switch (cat)
                    {
                        case 0:
                            return (context.Objects.Except(U.extent)).ToList<string>();
                        case 1:
                            return (context.Attributes.Except(U.intent)).ToList<string>();
                        case 2:
                            return (context.Conditions.Except(U.modus)).ToList<string>();
                        default:
                            throw new ParseException("Error!");
                    }
                }
            }
            else
            {
                throw new ParseException("'+' or '-' expected");
            }
        }

        /*private double parseC(string domain) // C -> domain
        {
            if (curLex.type != (int)LexType.LEX_DOMAIN)
            {
                throw new ParseException("Domain name expected");
            }
            else
            {
                int num = 0;
                for(int i = 0; i < curLex.buf.Length; i++)
                {
                    num += ((int)(Convert.ToChar((curLex.buf[i].ToString()).ToLower())) - (int)('a')) * (int)Math.Pow(26.0, (double)curLex.buf.Length - (double)i - 1.0);
                }
                if (num > context.DomainNum)
                {
                    throw new ParseException("Domain not found");
                }
            }
            GetNextLexeme();
        }*/
    }
}