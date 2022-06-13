using System;
using System.Threading;
using System.IO;

namespace Simulator
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.IO;
    //using java.io;

    namespace Simulator
    {

/*
        class SharableSpreadaheet
        {

            private List<List<string>> sheet;
            private List<Mutex> rowsReadersCounter;
            private List<int> Row_reader_counter;
            private Semaphore allSheet;
            private Mutex readerCounter;
            private Mutex writerCounter;
            private Semaphore limitUsers;
            private int rCounter;
            private int wCounter;
            private int rowSize;
            private int colSize;

            public SharableSpreadaheet(int nRows, int nCols)
            {
                if (nRows <= 0 || nCols <= 0)
                    throw new ArgumentException("one or two of the parameters are below or equal zero");
                rowSize = nRows;
                colSize = nCols;
                rCounter = 0;
                limitUsers = new Semaphore(int.MaxValue, int.MaxValue);
                allSheet = new Semaphore(1, 5);
                readerCounter = new Mutex();
                writerCounter = new Mutex();
                wCounter = 0;
                this.sheet = new List<List<string>>();
                for (int i = 0; i < nRows; i++)
                {
                    this.sheet.Add(new List<string>());
                    for (int j = 0; j < nCols; j++)
                    {
                        this.sheet[i].Add("");
                    }
                }

                this.Row_reader_counter = new List<int>();
                for (int i = 0; i < nRows; i++)
                {
                    this.Row_reader_counter.Add(0);
                }
                this.rowsReadersCounter = new List<Mutex>();
                for (int i = 0; i < nRows; i++)
                {
                    this.rowsReadersCounter.Add(new Mutex());
                }
            }


            public String getCell(int row, int col)
            {
                if (row < 1 || col < 1)
                    return null;
                if (row > rowSize + 1 || col > colSize + 1)
                    return null;
                row--;
                col--;
                String save = "";
                limitUsers.WaitOne();
                readRow(row);
                save = this.sheet[row][col];
                exitRow(row);
                limitUsers.Release();
                return save;
            }
            public bool setCell(int row, int col, String str)
            {
                if (row < 1 || col < 1)
                    return false;
                if (row > rowSize + 1 || col > colSize + 1)
                    return false;
                row--;
                col--;
                limitUsers.WaitOne();
                writerCounter.WaitOne();
                if (rCounter == 0 && wCounter == 0)
                {
                    this.allSheet.WaitOne();
                }
                wCounter++;
                writerCounter.ReleaseMutex();
                while (Row_reader_counter[row] > 0) { }
                this.sheet[row][col] = str;
                writerCounter.WaitOne();
                wCounter--;
                if (rCounter == 0 && wCounter == 0)
                {
                    this.allSheet.Release();
                }
                writerCounter.ReleaseMutex();
                limitUsers.Release();
                return true;
            }
            public bool searchString(String str, ref int row, ref int col)
            {
                limitUsers.WaitOne();
                for (int i = 0; i < rowSize; i++)
                {
                    readRow(i);
                    for (int j = 0; j < colSize; j++)
                    {
                        if (this.sheet[i][j] == str)
                        {
                            row = i + 1;
                            col = j + 1;
                            exitRow(i);
                            limitUsers.Release();
                            return true;
                        }

                    }
                    exitRow(i);
                }
                limitUsers.Release();
                return false;
            }
            public bool exchangeRows(int row1, int row2)
            {
                if (row1 < 1 || row2 < 1)
                    return false;
                if (row1 > rowSize + 1 || row2 > rowSize + 1)
                    return false;
                row1--;
                row2--;
                limitUsers.WaitOne();
                writerCounter.WaitOne();
                if (rCounter == 0 && wCounter == 0)
                {
                    this.allSheet.WaitOne();
                }
                wCounter++;
                writerCounter.ReleaseMutex();
                while (Row_reader_counter[row1] > 0 || Row_reader_counter[row2] > 0) { }
                for (int i = 0; i < colSize; i++)
                {

                    String temp = this.sheet[row1][i];
                    this.sheet[row1][i] = this.sheet[row2][i];
                    this.sheet[row2][i] = temp;

                }
                writerCounter.WaitOne();
                wCounter--;
                if (rCounter == 0 && wCounter == 0)
                {
                    this.allSheet.Release();
                }
                writerCounter.ReleaseMutex();
                limitUsers.Release();
                return true;
            }
            public bool exchangeCols(int col1, int col2)
            {
                col1--;
                col2--;
                if (col1 < 0 || col2 < 0)
                    return false;
                if (col1 > colSize || col2 > colSize)
                    return false;
                limitUsers.WaitOne();
                writerCounter.WaitOne();
                if (rCounter == 0 && wCounter == 0)
                {
                    this.allSheet.WaitOne();
                }
                wCounter++;
                writerCounter.ReleaseMutex();
                for (int i = 0; i < rowSize; i++)
                {
                    while (Row_reader_counter[i] > 0) { }
                    String temp = this.sheet[i][col1];
                    this.sheet[i][col1] = this.sheet[i][col2];
                    this.sheet[i][col2] = temp;
                }
                writerCounter.WaitOne();
                wCounter--;
                if (rCounter == 0 && wCounter == 0)
                {
                    this.allSheet.Release();
                }
                writerCounter.ReleaseMutex();
                limitUsers.Release();
                return true;
            }
            public bool searchInRow(int row, String str, ref int col)
            {
                row--;
                limitUsers.WaitOne();
                readRow(row);
                for (int i = 0; i < colSize; i++)
                {

                    if (this.sheet[row][i] == str)
                    {
                        col = i + 1;
                        exitRow(row);
                        limitUsers.Release();
                        return true;
                    }

                }
                exitRow(row);
                limitUsers.Release();
                return false;
            }
            public bool searchInCol(int col, String str, ref int row)
            {
                col--;
                limitUsers.WaitOne();
                for (int i = 0; i < rowSize; i++)
                {
                    readRow(i);
                    if (this.sheet[i][col] == str)
                    {
                        row = i + 1;
                        exitRow(i);
                        limitUsers.Release();

                        return true;
                    }
                    exitRow(i);
                }
                limitUsers.Release();

                return false;
            }
            public bool searchInRange(int col1, int col2, int row1, int row2, String str, ref int row, ref int col)
            {
                col1--;
                col2--;
                row1--;
                row2--;
                if (col1 > col2 || row1 > row2 || row2 < 0 || row2 > rowSize || col2 < 0 || col2 > colSize)
                    return false;
                limitUsers.WaitOne();
                for (int i = row1; i < row2 + 1; i++)
                {
                    readRow(i);
                    for (int j = col1; j < col2 + 1; j++)
                    {
                        if (this.sheet[i][j] == str)
                        {
                            row = i + 1;
                            col = j + 1;
                            exitRow(i);
                            limitUsers.Release();
                            return true;
                        }
                    }
                    exitRow(i);
                }
                limitUsers.Release();
                return false;
            }

            public bool addRow(int row1)
            {
                row1--;
                if (row1 < 0 || row1 > rowSize)
                    return false;
                allSheet.WaitOne();
                sheet.Insert(row1, new List<string>());
                for (int i = 0; i < colSize; i++)
                {
                    sheet[row1].Add("");
                }
                rowsReadersCounter.Insert(row1, new Mutex());
                Row_reader_counter.Insert(row1, 0);
                rowSize++;
                allSheet.Release();
                return true;
            }
            public bool addCol(int col1)
            {
                col1--;
                if (col1 < 0 || col1 > rowSize)
                    return false;
                allSheet.WaitOne();

                for (int i = 0; i < rowSize; i++)
                {
                    sheet[i].Insert(col1, "");
                }
                colSize++;
                allSheet.Release();
                return true;
            }
            public void getSize(ref int nRows, ref int nCols)
            {
                nRows = rowSize;
                nCols = colSize;

            }
            public bool setConcurrentSearchLimit(int nUsers)
            {
                // this function aims to limit the number of users that can perform the search operations concurrently.
                // The default is no limit. When the function is called, the max number of concurrent search operations is set to nUsers. 
                // In this case additional search operations will wait for existing search to finish.
                if (nUsers < 1)
                    return false;
                allSheet.WaitOne();
                limitUsers = new Semaphore(nUsers, nUsers + 20);// need to check !!!!!!!!!!!!********!!!!!!!TODO!!!!!!****
                allSheet.Release();
                return true;
            }

            public bool save(String fileName)
            {
                // save the spreadsheet to a file fileName.
                // you can decide the format you save the data. There are several options.
                if (fileName == null || fileName == "")
                    return false;
                allSheet.WaitOne();
                StreamWriter streamW = new StreamWriter(fileName);
                streamW.Write(rowSize + "\n");
                streamW.Write(colSize + "\n");
                for (int i = 0; i < rowSize; i++)
                {
                    for (int j = 0; j < colSize; j++)
                    {
                        streamW.Write(sheet[i][j] + "\n");
                    }

                }
                streamW.Flush();
                streamW.Close();
                allSheet.Release();
                return true;
            }
            public bool load(String fileName)
            {
                // load the spreadsheet from fileName
                // replace the data and size of the current spreadsheet with the loaded data
                allSheet.WaitOne();
                StreamReader streamR = new StreamReader(fileName);
                int row = Int32.Parse(streamR.ReadLine());
                int col = Int32.Parse(streamR.ReadLine());
                List<List<string>> newSheet = new List<List<string>>();
                string line;
                for (int i = 0; i < row; i++)
                {
                    newSheet.Add(new List<string>());
                    for (int j = 0; j < col; j++)
                    {
                        line = streamR.ReadLine();
                        newSheet[i].Add(line);
                    }
                }
                streamR.Close();
                rowSize = row;
                colSize = col;
                sheet = newSheet;
                allSheet.Release();
                return true;
            }

            private void readRow(int i)
            {
                this.readerCounter.WaitOne();
                if (rCounter == 0 && wCounter == 0)
                {
                    this.allSheet.WaitOne();
                }
                rCounter++;
                this.readerCounter.ReleaseMutex();
                rowsReadersCounter[i].WaitOne();
                Row_reader_counter[i]++;

                rowsReadersCounter[i].ReleaseMutex();
            }
            private void exitRow(int i)
            {
                rowsReadersCounter[i].WaitOne();
                Row_reader_counter[i]--;

                rowsReadersCounter[i].ReleaseMutex();
                this.readerCounter.WaitOne();
                rCounter--;
                if (rCounter == 0 && wCounter == 0)
                    this.allSheet.Release();
                this.readerCounter.ReleaseMutex();
            }
        }
*/


        class Program
        {
            static void Main(string[] args)
            {
                int rows = int.Parse(args[0]);
                int cols = int.Parse(args[1]);
                int nThreads = int.Parse(args[2]);
                int nOperations = int.Parse(args[3]);
                SharableSpreadsheet s = new SharableSpreadsheet(rows, cols);
                //ThreadPool.SetMinThreads(2, 2);
                s.setConcurrentSearchLimit(nThreads);
                ThreadPool.SetMaxThreads(nThreads, nThreads);
                string defStr = "testcell";
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        s.setCell(i+1, j+1, defStr + (i+1) + (j+1));
                    }
                }
                for (int i = 0; i < nThreads; i++)
                {
                    ThreadPool.QueueUserWorkItem(state => oneUser(s, nOperations, nThreads));
                }
                int max, max2;
                ThreadPool.GetMaxThreads(out max, out max2);
                int available, available2;
                ThreadPool.GetAvailableThreads(out available, out available2);
                while (max - available != 0)
                {
                    ThreadPool.GetMaxThreads(out max, out max2);
                    ThreadPool.GetAvailableThreads(out available, out available2);
                }
                // s.setCell(1,1,"noa");
                s.save("spreadsheet.dat");

            }
            public static void oneUser(SharableSpreadsheet s, int nOperations, int nThreads)
            {
                Random r = new Random();
                int row = 0, col = 0;
                int funcNum = 0;
                s.getSize(ref row, ref col);
                for (int i = 0; i < nOperations; i++)
                {
                    funcNum = r.Next(0,13);
                    if (funcNum == 0)
                    {
                        int randRow = r.Next(1, row + 1);
                        int randCol = r.Next(1, col + 1);
                        //Console.WriteLine("start getCell");
                        String res = s.getCell(randRow, randCol);
                        Console.WriteLine("User [" + Thread.CurrentThread.ManagedThreadId + "]: " + "in cell " + "[" + randRow + "," + randCol + "]" + " found: " + res);
                        Thread.Sleep(100);
                        continue;
                    }
                    if (funcNum == 1)
                    {
                        int randRow = r.Next(1, row + 1);
                        int randCol = r.Next(1, col + 1);
                        //Console.WriteLine("start setCell");
                        if (s.setCell(randRow, randCol, "testcell" + r.Next(0, row) + r.Next(0, col)))
                            Console.WriteLine("User [" + Thread.CurrentThread.ManagedThreadId + "]: " + "cell: " + "[" + randRow + "," + randCol + "]" + " changed successfully");
                        else
                            Console.WriteLine("User [" + Thread.CurrentThread.ManagedThreadId + "]: " + "problem in set cell");
                        Thread.Sleep(100);
                        continue;
                    }
                    if (funcNum == 2)
                    {
                        int randRow = r.Next(1, row + 1);
                        int randCol = r.Next(1, col + 1);
                        int thisRow = 0;
                        int thisCol = 0;
                       // Console.WriteLine("start searchString");
                        if (s.searchString("testcell" + randRow + randCol, ref thisRow, ref thisCol))
                            Console.WriteLine("User [" + Thread.CurrentThread.ManagedThreadId + "]: " + "string 'testcell" + randRow + randCol + "'" + " found in cell: " + "[" + randRow + "," + randCol + "]");
                        else
                            Console.WriteLine("User [" + Thread.CurrentThread.ManagedThreadId + "]: " + "didn't found : " + "testcell" + randRow + randCol);
                        Thread.Sleep(100);
                        continue;
                    }
                    if (funcNum == 10)
                    {
                        int randRow1 = r.Next(1, row + 1);
                        int randRow2 = r.Next(1, row + 1);
                         //Console.WriteLine("start exchangeRows");
                        if (s.exchangeRows(randRow1, randRow2))
                            Console.WriteLine("User [" + Thread.CurrentThread.ManagedThreadId + "]: " + "rows: " + randRow1 + "," + randRow2 + " changed successfully");
                        else
                            Console.WriteLine("User [" + Thread.CurrentThread.ManagedThreadId + "]: " + "exchange rows failed : " + randRow1 + "," + randRow2);
                        Thread.Sleep(100);
                        continue;
                    }
                    if (funcNum == 11)
                    {
                        int randCol1 = r.Next(1, col + 1);
                        int randCol2 = r.Next(1, col + 1);
                       // Console.WriteLine("start exchangeCols");
                        if (s.exchangeCols(randCol1, randCol2))
                            Console.WriteLine("User [" + Thread.CurrentThread.ManagedThreadId + "]: " + "columns: " + randCol1 + "," + randCol2 + " changed successfully");
                        else
                            Console.WriteLine("User [" + Thread.CurrentThread.ManagedThreadId + "]: " + "exchange columns failed: " + randCol1 + "," + randCol2);
                        Thread.Sleep(100);
                        continue;
                    }
                    if (funcNum == 3)
                    {
                        int row2 = r.Next(1, row + 1);
                        int foundCol = 0;
                        int randRow = r.Next(1, row + 1);
                        int randCol = r.Next(1, col + 1);
                        //Console.WriteLine("start searchInRow");
                        if (s.searchInRow(row2, "testcell" + randRow + randCol, ref foundCol))
                            Console.WriteLine("User [" + Thread.CurrentThread.ManagedThreadId + "]: " + "string 'testcell" + randRow + randCol + "'" + " found in cell: " + "[" + row2 + "," + foundCol + "]");
                        else
                            Console.WriteLine("User [" + Thread.CurrentThread.ManagedThreadId + "]: " + "not found");
                        Thread.Sleep(100);
                        continue;
                    }
                    if (funcNum == 4)
                    {
                        int col2 = r.Next(1, col + 1);
                        int foundRow = 0;
                        int randRow = r.Next(1, row + 1);
                        int randCol = r.Next(1, col + 1);
                         //Console.WriteLine("start searchInCol");
                        if (s.searchInCol(col2, "testcell" + randRow + randCol, ref foundRow))
                            Console.WriteLine("User [" + Thread.CurrentThread.ManagedThreadId + "]: " + "string 'testcell" + randRow + randCol + "'" + " found in cell: " + "[" + foundRow + "," + col2 + "]");
                        else
                            Console.WriteLine("User [" + Thread.CurrentThread.ManagedThreadId + "]: " + "not found");
                        Thread.Sleep(100);
                        continue;
                    }
                    if (funcNum == 5)
                    {
                        int randRow1 = r.Next(1, row + 1);
                        int randRow2 = r.Next(1, row + 1);
                        int randCol1 = r.Next(1, col + 1);
                        int randCol2 = r.Next(1, col + 1);
                        int strRow = r.Next(1, randRow2 + 1);
                        int strCol = r.Next(1, randCol2 + 1);
                        int resRow = 1;
                        int resCol = 1;
                       // Console.WriteLine("start searchInRange");
                        if (s.searchInRange(randCol1, randCol2, randRow1, randRow2, "testcell" + strRow + strCol, ref resRow, ref resCol))
                            Console.WriteLine("User [" + Thread.CurrentThread.ManagedThreadId + "]: " + "string 'testcell" + strRow + strCol + "'" + " found in cell: " + "[" + resRow + "," + resCol + "]");
                        else
                            Console.WriteLine("User [" + Thread.CurrentThread.ManagedThreadId + "]: " + "not found");
                        Thread.Sleep(100);
                        continue;
                    }
                    if (funcNum == 6)
                    {
                        int randRow1 = r.Next(1, row + 1);
                        // Console.WriteLine("start addRow");
                        if (s.addRow(randRow1))
                        {
                            Console.WriteLine("User [" + Thread.CurrentThread.ManagedThreadId + "]: " + "a new row added after row " + randRow1);
                            row++;
                        }
                        else
                            Console.WriteLine("User [" + Thread.CurrentThread.ManagedThreadId + "]: " + "adding row failed");
                        Thread.Sleep(100);
                        continue;
                    }
                    if (funcNum == 7)
                    {
                        int randCol = r.Next(1, col + 1);
                        // Console.WriteLine("start addCol");
                        if (s.addCol(randCol))
                        {
                            Console.WriteLine("User [" + Thread.CurrentThread.ManagedThreadId + "]: " + "a new column added after column " + randCol);
                            col++;
                        }
                        else
                            Console.WriteLine("User [" + Thread.CurrentThread.ManagedThreadId + "]: " + "adding column failed");
                        Thread.Sleep(100);
                        continue;
                    }
                    if (funcNum == 8)
                    {
                        int resRow = 0;
                        int resCol = 0;
                         // Console.WriteLine("start getSize");
                        s.getSize(ref resRow, ref resCol);
                        Console.WriteLine("User [" + Thread.CurrentThread.ManagedThreadId + "]: " + "sheet size: " + "rows: " + resRow + ", colmuns: " + resCol);
                        Thread.Sleep(100);
                        continue;
                    }
                    if (funcNum == 12)
                    {
                        int randUsers = r.Next(1, nThreads + 1);
                        // Console.WriteLine("start setConcurrentSearchLimit");
                        //ThreadPool
                        if (s.setConcurrentSearchLimit(randUsers))
                            Console.WriteLine("User [" + Thread.CurrentThread.ManagedThreadId + "]: " + "limit users changed to " + " " + randUsers + " successfuly");
                        else
                            Console.WriteLine("User [" + Thread.CurrentThread.ManagedThreadId + "]: " + "limit users change failed");
                        Thread.Sleep(100);
                        continue;
                    }
                    if (funcNum == 9)
                    {
                         // Console.WriteLine("start save");
                        if (s.save("spreadsheet.dat"))
                            Console.WriteLine("User [" + Thread.CurrentThread.ManagedThreadId + "]: " + "saved spreadsheet to file");
                        else
                            Console.WriteLine("User [" + Thread.CurrentThread.ManagedThreadId + "]: " + "save failed");
                        Thread.Sleep(100);
                        continue;

                    }

                }
            }

        }
    }
    }

