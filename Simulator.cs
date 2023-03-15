using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharableSpreadSheet;

namespace SharableSpreadSheet.cs
{
    internal class Simulator
    {
        public static int rows;
        public static int cols;
        public static int nThreads;
        public static void Main(string[] args)
        {
            rows = int.Parse(args[0]); ;
            cols = int.Parse(args[1]);
            int nThreads = int.Parse(args[2]);
            int nOperations= int.Parse(args[3]); ;
            int mssleep= int.Parse(args[4]); ;
            int counter = 0;
            int nUsers = 10;//init users amount

            SharableSpreadSheet spreadsheet = new SharableSpreadSheet(rows, cols, nUsers);
            spreadsheet.save("spreadsheetNEW.txt");
            spreadsheet.load("spreadsheetNEW.txt");
            //spreadsheet.setConcurrentSearchLimit(10);

            ThreadPool.SetMinThreads(nThreads, nThreads);
            ThreadPool.SetMaxThreads(nThreads,nThreads);
            //initilize defaulte param when save file

            while(counter < nOperations)
            {
                for(int i=0; i<nThreads; i++)
                {
                    ThreadPool.QueueUserWorkItem(Thread => randomFunc(spreadsheet));
                }
                counter++;
            }
          
            Thread.Sleep(5000);

            
            /*Console.WriteLine("-------------------------");
            Console.WriteLine("Table state after running: ");
            SharableSpreadSheet.prinTable(spreadsheet);*/
        }
        
           
            private static void randomFunc(object newObj)
        {
            SharableSpreadSheet spreadSheet = (SharableSpreadSheet)newObj;
            Random random = new Random();
            int randNum = random.Next(1, 11);
            //int randNum = 13;
            int row = 0;
            int col = 0;
           
           
            switch(randNum)
            {
                case 1:
                    
                    row = random.Next(0, rows);
                    col = random.Next(0, cols);
                   
                    String str = spreadSheet.getCell(row, col);
                    
                        Console.WriteLine("User[" + Thread.GetCurrentProcessorId()
                            +"]"+":"+"["+DateTime.Now+"] "+"<"+ str+">"+ " found in cell ["
                            +row+","+col+"]");
                    Thread.Sleep(1000);

                    break;

                case 2:
                    
                    row = random.Next(0, rows);
                    col = random.Next(0, cols);
                    Thread.Sleep(2000);
                    spreadSheet.setCell(row, col, "idan");
                    //massage send from setCell function
                    Thread.Sleep(1000);
                    break;

                case 3:
                    
                    String s1 = "idan";
                    Tuple<int, int> tuple = spreadSheet.searchString("idan");
                   
                     
                    if (tuple == null)
                    {
                        Console.WriteLine("User[" + Thread.GetCurrentProcessorId()
                                + "]" + ":" + "[" + DateTime.Now + "] " +  
                                "String" +"<"+s1+ ">"+"not found in the cell");
                    }
                    else
                    {
                        Console.WriteLine("User[" + Thread.GetCurrentProcessorId()
                                + "]" + ":" + "[" + DateTime.Now + "] " + "the string" +
                                s1 +" found at: "
                                + "<" + tuple.Item1
                                + "," + tuple.Item2 + ">" + " cell");
                    }
                    Thread.Sleep(1000);
                    break;

                case 4:
                    
                    int row1 = random.Next(0, rows);
                    int row2 = random.Next(0, cols);
                       
                    spreadSheet.exchangeRows(row1, row2);
                    Thread.Sleep(1000);

                    break;

                case 5:
                
                    int col1 = random.Next(0, rows);
                    int col2 = random.Next(0, cols);
                    spreadSheet.exchangeCols(col1, col2);
                    Thread.Sleep(1000);
                    break;

                case 6:
                  
                    row2 = random.Next(0, rows);
                    String str3 = "a";
                    int res = spreadSheet.searchInRow(row2, "a");
                    if (res == -1)
                    {
                        Console.WriteLine("User[" + Thread.GetCurrentProcessorId()
                                + "]" + ":" + "[" + DateTime.Now + "] " +
                                "string " + "<" + str3 + ">" + " not found in this row");
                    }
                    else
                    {
                        Console.WriteLine("User[" + Thread.GetCurrentProcessorId()
                                + "]" + ":" + "[" + DateTime.Now + "] " + "String " +
                                "<" +str3+">" + " found at row: " + row2 + " col: " + res);
                    }
                    Thread.Sleep(5000);

                    break;

                case 7:
                   
                    col2 = random.Next(0, rows);
                    String str4 = "a";
                    int res1 = spreadSheet.searchInRow(col2, "a");
                    if (res1 == -1)
                    {
                        Console.WriteLine("User[" + Thread.GetCurrentProcessorId()
                                + "]" + ":" + "[" + DateTime.Now + "] " +
                                "string" + "<" + str4 + ">" + "not found in this col");
                    }
                    else
                    {
                        Console.WriteLine("User[" + Thread.GetCurrentProcessorId()
                                + "]" + ":" + "[" + DateTime.Now + "] " +
                                "String " + "<" + str4 + ">" +
                                " found at col: " + col2 + " col: " + res1);
                    }
                    Thread.Sleep(1000);

                    break;

                case 8:
                    String s5 = "a";
                    Tuple<int, int> tuple1 = spreadSheet.searchInRange(0, cols, 0, rows, s5);
                    if (tuple1.Item1 == -1)
                    {
                        Console.WriteLine("User[" + Thread.GetCurrentProcessorId()
                               + "]" + ":" + "[" + DateTime.Now + "] " + "The string"+
                               "<" + s5 + ">" + "is not found");
                    }
                    else
                    {
                        Console.WriteLine("User[" + Thread.GetCurrentProcessorId()
                                   + "]" + ":" + "[" + DateTime.Now + "] " + "the string " +
                                   "<"+s5 +">"+ " found at: "
                                   + "<" + tuple1.Item1
                                   + "," + tuple1.Item2 + ">" + " cell");
                    }
                    Thread.Sleep(1000);
                    break;
                     
                case 9:
                    int row9 = random.Next(0, rows);
                    spreadSheet.addRow(row9);
                    Console.WriteLine("User[" + Thread.GetCurrentProcessorId()
                                  + "]" + ":" + "[" + DateTime.Now + "] " +
                                  "add a row after row: " + row9 + " is completed");
                    Thread.Sleep(1000);
                    break;

                case 10:
                    int col10 = random.Next(0, rows);
                    spreadSheet.addRow(col10);
                    Console.WriteLine("User[" + Thread.GetCurrentProcessorId()
                                  + "]" + ":" + "[" + DateTime.Now + "] " +
                                  "add a column after column: " + col10 + "is completed.");
                    Thread.Sleep(1000);
                    break;

                case 11:
                    
                    Console.WriteLine("Thread insert case 11");
                    String s11 = "a";                  
                    Tuple<int, int>[] arr = spreadSheet.findAll(s11, true);
                    Console.WriteLine("User[" + Thread.GetCurrentProcessorId()
                                  + "]" + ":" + "[" + DateTime.Now + "] " +
                                  "all cells with "+ "<"+s11+">" +" found");
                    Thread.Sleep(1000);
                    break;

                case 12:
                    String s12 = "a";
                    String s13 = "b";
                    spreadSheet.setAll(s12, s13, true);
                    Console.WriteLine("User[" + Thread.GetCurrentProcessorId()
                                  + "]" + ":" + "[" + DateTime.Now + "] " +
                                  "new string as been set");
                    Thread.Sleep(1000);
                    break;

                case 13:
                    
                    Tuple<int, int> t13 = spreadSheet.getSize();
                    Console.WriteLine("User[" + Thread.GetCurrentProcessorId()
                                  + "]" + ":" + "[" + DateTime.Now + "] "+
                                  "size of spreadsheet: "+ "number of rows- "+
                                  t13.Item1+"," +"number of cols- "+ t13.Item2);
                    Thread.Sleep(1000);
                    break;










            }
        }
    }
}
