using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;

namespace SharableSpreadSheet.cs
{
    using System;
    class SharableSpreadSheet
    {
        
        static Semaphore readsLimit;
        private static int _padding;
        int rows;
        int cols;
        int nUsers;
        String[,] table;
     
        Mutex mut;
        Mutex[,] muTable;
        Mutex ifWriteToCell;
        static bool wrintingToCell;
        static bool lockAccessTable;
        static Semaphore _pool = null;
        public SharableSpreadSheet(int nRows, int nCols, int nUsers = -1)
        {
            // nUsers used for setConcurrentSearchLimit, -1 mean no limit.
            // construct a nRows*nCols spreadsheet
            this.rows = nRows;
            this.cols = nCols;
            this.nUsers = nUsers;
            this.setConcurrentSearchLimit(nUsers); 

            table = new String[nRows, nCols];
            wrintingToCell = false;
            lockAccessTable = false;
            

            ifWriteToCell = new Mutex();

            mut = new Mutex();
            muTable = new Mutex[rows,cols];
            for(int i = 0; i < rows; i++)
            {
                for(int j = 0; j < cols; j++)
                {
                    muTable[i,j] = new Mutex();
                }
            }
                    
            nUsers = 10;
            _pool = new Semaphore(0, 1);
            _pool.Release();
        }

        public static void prinTable(SharableSpreadSheet sh)
        {
            bool flag = true;
            Console.WriteLine("----------------------");
            for (int g = 0; g < sh.rows; g++)
            {
                Console.WriteLine("");
                Console.Write("[");
                for (int h = 0; h < sh.cols; h++)
                {
                    Console.Write(sh.table[g, h]+ " ");

                }
                Console.Write("]");
            }
            Console.WriteLine(" "); 
            Console.WriteLine("----------------------");

        }          
        public String getCell(int row, int col)//case 1
        {
            String str=null;
           
            readsLimit.WaitOne();
            
            bool flag = false;
            if (lockAccessTable)
            {
                flag = true;
                _pool.WaitOne();
            }
            
            if (row >= this.rows || col >= this.cols)
            {
                throw new ArgumentOutOfRangeException("Can't get this cell");               
            }

            if (wrintingToCell)
            {
                muTable[row, col].WaitOne();              
               
                str = table[row, col];
                muTable[row, col].ReleaseMutex();
            }
            else
            {
                
                str = table[row, col];
            }
            if (flag)
            {
                _pool.Release();
            }
           
            readsLimit.Release();
            return str;
        }

        public void setCell(int row, int col, String str)
        {
            bool flag = false;
            if (lockAccessTable)
            {
                flag = true;
                _pool.WaitOne();
            }
            // set the string at [row,col]
            if (row >= this.rows || col >= this.cols)
            {
                throw new ArgumentOutOfRangeException("Can't get this cell");
            }
            //if user write to table we must lock the cell and lock reading from cell
            //when another user reading at the same time from same cell
            
      
            wrintingToCell = true;
            muTable[row, col].WaitOne();           
            table[row,col] = str;
            SharableSpreadSheet.printWritingMassage(str,col,row);
            muTable[row,col].ReleaseMutex();
            wrintingToCell = false;
          
            if (flag)
            {
                _pool.Release();
            }

        }
        public static void printWritingMassage(string str,int col, int row)
        {
            Console.WriteLine("User[" + Thread.GetCurrentProcessorId()
                            + "]" + ":" + "[" + DateTime.Now + "] " + "<" + str + ">" + " insert to cell ["
                            + row + "," + col + "]");
        }

        public Tuple<int, int> searchString(String str)//case 3
        {
            bool flag = false;
            if (lockAccessTable)
            {
                flag = true;
                _pool.WaitOne();
            }
       
            int row, col;
            
            Tuple<int, int> tuple = new Tuple<int, int>(this.rows, this.cols);
            int tupleRow = tuple.Item1;
            int tupleCol = tuple.Item2;
            if (tupleRow > this.rows || tupleCol > this.cols)
            {
                //Console.WriteLine(tuple.Item1);
                //Console.WriteLine(tuple.Item2);
                throw new ArgumentOutOfRangeException("Can't get this cell");
            }
            Tuple<int, int> tupleToReturn = null;
            //Console.WriteLine("search string");
            for (row = 0; row < tupleRow; row++)
            {               
                for (col = 0; col < tupleCol; col++)
                {
                    //Thread.Sleep(2000);
                    //read cell by getCell function logic and continue
                    String s = getCell(row, col);
                    if (s.Equals(str)) {
                        tupleToReturn = new Tuple<int,int>(row,col);
                        return tupleToReturn;
                    }
             
                }
            }
            if (flag)
            {
                _pool.Release();
            }

            return null;
        }
        public void exchangeRows(int row1, int row2)//case 4
        {
            _pool.WaitOne();
            lockAccessTable = true;          
            String[] arrayString = new String[rows * cols+1000];
          
            // exchange the content of row1 and row2
            
            for (int i = 0; i < this.cols; i++)
            {                       
                arrayString[i] = table[row1, i];
    
            }
            
            for (int i = 0; i < this.cols; i++)
            {
                table[row1, i] = table[row2, i];                              
                table[row2, i] = arrayString[i];                
            }
            SharableSpreadSheet.printRowsAfterExchange(this,row1,row2);
            //Thread.Sleep(2000);
            _pool.Release();
            lockAccessTable = false;
        }
        public static void printRowsAfterExchange(SharableSpreadSheet sh, int row1,int row2)
        {
            Console.WriteLine("User[" + Thread.GetCurrentProcessorId()
                               + "]" + ":" + "[" + DateTime.Now + "] " + "rows "+
                              "[" + row1 + "]" + " and " + "[" + row2 + "] exchanged successfully.");
            //SharableSpreadSheet.prinTable(sh);
        }
        public void exchangeCols(int col1, int col2)//case 5
        {
            _pool.WaitOne();
            lockAccessTable = true;
            String[] arrayString = new String[rows * cols];
            // exchange the content of col1 and col2

            for (int i = 0; i < rows; i++)
            {
                arrayString[i] = table[i, col1];
                //Console.WriteLine(table[i,col1]);
                //Console.WriteLine(arrayString[i]);
            }
            for (int i = 0; i < rows; i++)
            {
                table[i, col1] = table[i,col2];

                //Console.WriteLine(table[i,col1]);
                table[i,col2] = arrayString[i];

            }
            SharableSpreadSheet.printColsAfterExchange(this,col1, col2);
            _pool.Release();
            lockAccessTable = false;

        }
        public static void printColsAfterExchange(SharableSpreadSheet sh, int col1, int col2)
        {
            Console.WriteLine("User[" + Thread.GetCurrentProcessorId()
                               + "]" + ":" + "[" + DateTime.Now + "] " + "columns " +
                              "[" + col1 + "]" + " and " + "[" + col2 + "] exchanged successfully.");
            //SharableSpreadSheet.prinTable(sh);
        }
        public int searchInRow(int row, String str)//case 6
        {          
            int col=0;
            // perform search in specific row
            for(int i = 0; i < cols; i++)
            {
                //read cell by getCell function logic and continue
                String s = getCell(row, i);
                if (s.Equals(str))
                {
                    col = i;
                    return col;
                }
            }
            return -1;
        }
        public int searchInCol(int col, String str)//7
        {
            int row=0;
            // perform search in specific col
            for (int i = 0; i < rows; i++)
            {
                String s = getCell(row, i);
                if (s.Equals(str))
                {
                    row = i;
                    return row;
                }
            }
            return -1;
        }
        public Tuple<int, int> searchInRange(int col1, int col2, int row1, int row2, String str)//8
        {
            _pool.WaitOne();
            lockAccessTable = true;
            int row, col;           
            // perform search within spesific range: [row1:row2,col1:col2] 
            //includes col1,col2,row1,row2
            
            for(int i = row1; i < row2; i++)
            {
                for(int j = col1; j < col2; j++)
                {
                    if(str.Equals(table[i, j]))
                    {
                        _pool.Release();
                        lockAccessTable = false;
                        return Tuple.Create(i, j);
                    }
                }
            }
            _pool.Release();
            lockAccessTable = false;
            return Tuple.Create(-1, -1);
        }
        public void addRow(int row1)// case 9
        {
            _pool.WaitOne();
            lockAccessTable = true;
            String[,] arrayString = new String[rows+1,cols];
            int g = 0;
            int h = 0;
            for (int i = 0; i < row1 + 1; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    arrayString[g, h] = table[i, j];
                    h++;
                }
                g++;
                h = 0;
            }
            for (int t = 0; t < cols; t++)
            {
                arrayString[g, t] = "";
            }
            g++;
            for (int i = row1+1; i < rows; ++i)
            {                
                for (int j = 0; j < cols; ++j)
                {                    
                    arrayString[g,h] = table[i, j];
                    h++;
                }
                g++;
                h = 0;
            }
            //add a row after row1
            rows++;
            this.table = new String[rows,cols];
            for(int i = 0; i < rows; i++)
            {
                for(int j = 0; j < cols; j++)
                {
                    this.table[i, j] = arrayString[i, j];
                }
            }
            _pool.Release();
            lockAccessTable = false;
        }
        public void addCol(int col1)//case 10
        {
            _pool.WaitOne();
            lockAccessTable = true;
            //add a column after col1
            String[,] arrayString = new String[rows, cols+1];
            int g = 0;
            int h = 0;
            for (int i = 0; i < col1 + 1; ++i)
            {
                for (int j = 0; j < rows; ++j)
                {

                    arrayString[g, h] = table[j, i];
                    g++;
                }
                h++;
                g = 0;
            }
            for (int t = 0; t < rows; t++)
            {
                arrayString[t, h] = "";
            }
            h++;
            for (int i = col1 + 1; i < cols; ++i)
            {
                for (int j = 0; j < rows; ++j)
                {

                    arrayString[g, h] = table[j, i];
                    g++;
                }
                h++;
                g = 0;
            }

            //add a col after col1
            this.cols++;
            this.table = new String[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    this.table[i, j] = arrayString[i, j];
                }
            }
            _pool.Release();
            lockAccessTable = false;
        }
        public Tuple<int, int>[] findAll(String str, bool caseSensitive)//case 12
        {
            bool flag = false;
            if (lockAccessTable)
            {
                flag = true;
                _pool.WaitOne();
            }
            Tuple<int, int>[] arrayOfTuples = new Tuple<int, int>[rows*cols];
            //LinkedList<Tuple<int, int>> arrayOfTuples = new LinkedList<Tuple<int, int>>();

            Tuple<int, int> tuple = new Tuple<int,int>(-1,-1);
            //Console.WriteLine("hey");
            int row1=0, col1=0;

            if (caseSensitive)//save my data base as lower case words.find with ToLower() if exist
                str = str.ToLower();
            
            //Console.WriteLine("hey");
            for (int i=0;i< rows; i++)
            {
                for(int j=0; j<cols; j++)
                {
                    //getCell function handle locks
                    //Thread.Sleep(1000);
                    String s = getCell(i, j);
                    if (s.Equals(str))
                    {
                        //Console.WriteLine("(" + i + "," + j + ")");                       
                        arrayOfTuples[i] = new Tuple<int, int>(i, j);
                    }
                }
            }
            //SharableSpreadSheet.printFoundCell(arrayOfTuples);
            if (flag)
                _pool.Release();

            return arrayOfTuples;
           
        }
        public static void printFoundCell(Tuple<int, int>[] arr)
        {
            Console.WriteLine(arr.Length);
            for (int i = 0; i < 12; i++)
            {
                //Thread.Sleep(5000);
                Console.WriteLine(arr[i]);
            }
            Thread.Sleep(1000);
            Console.WriteLine("finish");
        }
        public void setAll(String oldStr, String newStr, bool caseSensitive)//case 13
        {
            _pool.WaitOne();
            lockAccessTable = false;
            //LinkedList<Tuple<int, int>> arrayOfTuples = new LinkedList<Tuple<int, int>>();
            Tuple<int, int>[] arrayOfTuples = new Tuple<int, int>[rows * cols];
            Tuple<int, int> tuple;
            // replace all oldStr cells with the newStr str according to caseSensitive param
            if(caseSensitive)//save my data base as lower case words 
                oldStr = oldStr.ToLower();

            arrayOfTuples = findAll(oldStr, caseSensitive);
            
            //Console.WriteLine(arrayOfTuples.Length);
            for (int i = 0;i < arrayOfTuples.Length;i++)
            {
                if (arrayOfTuples[i] == null)
                    break;
                //Console.WriteLine(arrayOfTuples[i]);
                tuple = arrayOfTuples[i];
                int row = tuple.Item1;
                int col = tuple.Item2;
                table[row,col] = newStr;

            }
            _pool.Release();
            lockAccessTable = false;
        }
        public Tuple<int, int> getSize()
        {
            if (table == null)
                return null;

            int nRows;
            int nCols;
            // return the size of the spreadsheet in nRows, nCols
            nRows = this.rows;
            nCols = this.cols;
            return new Tuple<int, int>(nCols, nRows);
            
        }
        public void setConcurrentSearchLimit(int nUsers)
        {
            // this function aims to limit the number of users that can perform the search operations concurrently.
            // The default is no limit. When the function is called, the max number of concurrent search operations is set to nUsers. 
            // In this case additional search operations will wait for existing search to finish.
            // This function is used just in the creation
            readsLimit = new Semaphore(0,nUsers);
            readsLimit.Release(nUsers);


            

        }

        public void save(String fileName)
        {
            // save the spreadsheet to a file fileName.
            // you can decide the format you save the data. There are several options.
            bool bo=true;
            String s;
            using (var sw = new StreamWriter(fileName))
            {
                sw.Write(table.GetLength(0) + " " + table.GetLength(1) + " \n");
                for (int i = 0; i < table.GetLength(0); i++)
                {
                    for (int j = 0; j < table.GetLength(1); j++)
                    {
                        if (bo)
                        {
                            s = "a";
                            bo= false;
                        }
                        else
                        {
                            s = "b";
                            bo= true;
                        }
                        sw.Write(table[i, j] + s + " ");
                    }
                    sw.Write("\n");
                }

                sw.Flush();
                sw.Close();
            }


        }
        public  void load(String fileName)
        {
            // load the spreadsheet from fileName
            // replace the data and size of the current spreadsheet with the loaded data
            int j = 0;
            int i = 0;
            using (var sr = new StreamReader(fileName))
            {

                string s = sr.ReadLine();
                s = sr.ReadLine();

                while (true)
                {

                    foreach (String row in s.Split('\n'))
                    {
                        j = 0;
                        foreach (String col in row.Split(' '))
                        {

                            table[i, j] = col;
                            j++;
                            if (j == cols)
                                break;
                        }
                        i++;
                        s = sr.ReadLine();
                        
                    }
                    if (i == this.rows)
                        break;
                }
                         

            }

    }

    }


}