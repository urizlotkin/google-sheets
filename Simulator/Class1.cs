using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
//using java.io;

namespace Simulator
{

    class AtomInt
    {
        private Mutex mux;
        private int val;

        public AtomInt(int startVal)
        {
            mux = new Mutex();
            val = startVal;
        }
        public void increment()
        {
            mux.WaitOne();
            val++;
            mux.ReleaseMutex();
        }
        public void decrement()
        {
            mux.WaitOne();
            val--;
            mux.ReleaseMutex();
        }
        public Boolean isBigger(int value)
        {
            mux.WaitOne();
            if (this.val > value)
            {
                mux.ReleaseMutex();
                return true;
            }
            mux.ReleaseMutex();
            return false;
        }
        public int getValue()
        {
            return this.val;
        }
    }


    class SharableSpreadsheet
    {

        private List<List<string>> sheet;
        private List<AtomInt> counterRowReders;
        private List<SemaphoreSlim> counterRowWriter;
        private SemaphoreSlim allSheet;
        private SemaphoreSlim limitUsers;
        private Mutex readerwriterCounter;
        private Mutex setMaxUsersInSearchingFunc;
        private volatile int rCounter;
        private volatile int wCounter;
        private int rowSize;
        private int colSize;
        private int limitInit;

        public SharableSpreadsheet(int nRows, int nCols)
        {
            if (nRows <= 0 || nCols <= 0)
                throw new ArgumentException("one or two of the parameters are below or equal zero");
            rowSize = nRows;
            colSize = nCols;
            rCounter = 0;
            limitUsers = new SemaphoreSlim(int.MaxValue, int.MaxValue);
            setMaxUsersInSearchingFunc = new Mutex();
            limitInit = int.MaxValue - 1000;
            allSheet = new SemaphoreSlim(1, 1);
            readerwriterCounter = new Mutex();
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
            this.counterRowReders = new List<AtomInt>();
            for (int i = 0; i < nRows; i++)
            {
                this.counterRowReders.Add(new AtomInt(0));
            }
            this.counterRowWriter = new List<SemaphoreSlim>();
            for (int i = 0; i < nRows; i++)
            {
                this.counterRowWriter.Add(new SemaphoreSlim(1,1));
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
            readRow(row);
            save = this.sheet[row][col];
            exitRow(row);
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
            readerwriterCounter.WaitOne();
            if (rCounter == 0 && wCounter == 0)
            {
                this.allSheet.Wait();
            }
            wCounter++;
            readerwriterCounter.ReleaseMutex();
            counterRowWriter[row].Wait();
            this.sheet[row][col] = str;
            counterRowWriter[row].Release();
            readerwriterCounter.WaitOne();
            wCounter--;
            if (rCounter == 0 && wCounter == 0 && allSheet.CurrentCount == 0)
            {
                this.allSheet.Release(1);
            }
            readerwriterCounter.ReleaseMutex();
            return true;
        }
        public bool searchString(String str, ref int row, ref int col)
        {
            limitUsers.Wait();
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
                        if (limitUsers.CurrentCount < limitInit)
                            limitUsers.Release();

                        return true;
                    }

                }
                exitRow(i);
            }
            if (limitUsers.CurrentCount < limitInit)
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
            allSheet.Wait();
            for (int i = 0; i < colSize; i++)
            {
                String temp = this.sheet[row1][i];
                this.sheet[row1][i] = this.sheet[row2][i];
                this.sheet[row2][i] = temp;
            }
            allSheet.Release(1);

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
            allSheet.Wait();
            for (int i = 0; i < rowSize; i++)
            {
                String temp = this.sheet[i][col1];
                this.sheet[i][col1] = this.sheet[i][col2];
                this.sheet[i][col2] = temp;
            }
            allSheet.Release(1);
            return true;
        }
        public bool searchInRow(int row, String str, ref int col)
        {
            row--;
            limitUsers.Wait();
            readRow(row);
            for (int i = 0; i < colSize; i++)
            {

                if (this.sheet[row][i] == str)
                {
                    col = i + 1;
                    exitRow(row);
                    if (limitUsers.CurrentCount < limitInit)
                        limitUsers.Release();
                    return true;
                }

            }
            exitRow(row);
            if (limitUsers.CurrentCount < limitInit)
                limitUsers.Release();
            return false;
        }
        public bool searchInCol(int col, String str, ref int row)
        {
            col--;
            limitUsers.Wait();
            for (int i = 0; i < rowSize; i++)
            {
                readRow(i);
                if (this.sheet[i][col] == str)
                {
                    row = i + 1;
                    exitRow(i);
                    if (limitUsers.CurrentCount < limitInit)
                        limitUsers.Release();
                    return true;
                }
                exitRow(i);
            }
            if (limitUsers.CurrentCount < limitInit)
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
            limitUsers.Wait();
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
                        if (limitUsers.CurrentCount < limitInit)
                            limitUsers.Release();
                        return true;
                    }
                }
                exitRow(i);
            }
            if (limitUsers.CurrentCount < limitInit)
                limitUsers.Release();
            return false;
        }

        public bool addRow(int row1)
        {
            row1--;
            if (row1 < 0 || row1 > rowSize)
                return false;
            allSheet.Wait();
            sheet.Insert(row1, new List<string>());
            for (int i = 0; i < colSize; i++)
            {
                sheet[row1].Add("");
            }
            counterRowReders.Insert(row1, new AtomInt(0));
            counterRowWriter.Insert(row1, new SemaphoreSlim(1,1));
            rowSize++;
            allSheet.Release(1);
            return true;
        }
        public bool addCol(int col1)
        {
            col1--;
            if (col1 < 0 || col1 > colSize)
                return false;
            allSheet.Wait();
            for (int i = 0; i < rowSize; i++)
            {
                sheet[i].Insert(col1, "");
            }
            colSize++;
            allSheet.Release(1);
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
            setMaxUsersInSearchingFunc.WaitOne();

            if (limitInit - limitUsers.CurrentCount > nUsers)
            {
                setMaxUsersInSearchingFunc.ReleaseMutex();
                return false;
            }
            limitUsers = new SemaphoreSlim(nUsers,nUsers);// need to check !!!!!!!!!!!!********!!!!!!!TODO!!!!!!****
            limitInit = nUsers;
            setMaxUsersInSearchingFunc.ReleaseMutex();
            return true;
        }

        public bool save(String fileName)
        {
            // save the spreadsheet to a file fileName.
            // you can decide the format you save the data. There are several options.
            if (fileName == null || fileName == "")
                return false;
            allSheet.Wait();
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
            allSheet.Wait();
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
            this.readerwriterCounter.WaitOne();
            if (rCounter == 0 && wCounter == 0)
            {
                this.allSheet.Wait();
            }
            rCounter++;
            counterRowReders[i].increment();
            this.readerwriterCounter.ReleaseMutex();
            if(!counterRowReders[i].isBigger(1))
            {
                counterRowWriter[i].Wait();
            }
        }
        private void exitRow(int i)
        {
            this.readerwriterCounter.WaitOne();
            rCounter--;
            if (rCounter == 0 && wCounter == 0 && allSheet.CurrentCount == 0)
            {
                this.allSheet.Release(1);            }
            counterRowReders[i].decrement();
            this.readerwriterCounter.ReleaseMutex();
            if (!(counterRowReders[i].isBigger(1)) && counterRowWriter[i].CurrentCount == 0)
            {
                counterRowWriter[i].Release(1);
            }
            
        }
    }

}
