using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MatrixMultiplicationLab
{
	internal class Program
	{
		private static readonly Random sRng = new Random();


		private static void Main(string[] args)
		{
			var ma = CreateEmptyMatrix(500, 600);
			var mb = CreateEmptyMatrix(600, 300);
			FillMatrix(ma);
			FillMatrix(mb);

			Measure(ma, mb, MatrixProductSequential, "Sequential");
			Measure(ma, mb, MatrixProductSimpleParallel, "Simple parallel");
			Measure(ma, mb, MatrixProductTransposedParallel, "Transposed parallel");
		}


		#region Utility

		private static double[][] CreateEmptyMatrix(int rows, int cols)
		{
			double[][] result = new double[rows][];

			for (int i = 0; i < rows; ++i)
			{
				result[i] = new double[cols];

				for (int j = 0; j < cols; j++)
				{
					result[i][j] = sRng.NextDouble() * 2;
				}
			}

			return result;
		}


		private static void FillMatrix(double[][] matrix)
		{
			var rows = matrix.Length;
			var cols = matrix[0].Length;

			for (int i = 0; i < rows; ++i)
			{
				for (int j = 0; j < cols; j++)
				{
					matrix[i][j] = sRng.NextDouble() * 2;
				}
			}
		}


		private static void PrintMatrix(double[][] matrix, int maxRows = 3, int maxColumns = 5)
		{
			var rows = Math.Min(matrix.Length, maxRows);
			var cols = Math.Min(matrix[0].Length, maxColumns);
			var rowPostfix = matrix[0].Length <= maxColumns ? "" : " ...";

			for (int i = 0; i < rows; ++i)
			{
				Console.WriteLine(string.Join(" ",
					from a in matrix[i].Take(cols)
					select string.Format("{0,7:F2}", a)
					) + rowPostfix);
			}

			if (matrix.Length > maxRows)
				Console.WriteLine(" ...");
		}


		private static void Measure(
			double[][] ma, double[][] mb, Func<double[][], double[][], double[][]> func, string name)
		{
			var sw = new Stopwatch();
			sw.Start();
			var prod = func(ma, mb);
			sw.Stop();
			PrintMatrix(prod);
			Console.WriteLine("{0}: {1} ms", name, sw.ElapsedMilliseconds);
		}

		#endregion


		#region Algorithms

		/// <summary>
		/// Sequential algorithm.
		/// </summary>
		private static double[][] MatrixProductSequential(double[][] matrixA, double[][] matrixB)
		{
			int aRows = matrixA.Length;
			int aCols = matrixA[0].Length;
			int bRows = matrixB.Length;
			int bCols = matrixB[0].Length;

			if (aCols != bRows)
				throw new ArgumentException("Mismatching sizes");

			var result = CreateEmptyMatrix(aRows, bCols);

			for (int i = 0; i < aRows; ++i) // each row of A
			{
				for (int j = 0; j < bCols; ++j) // each col of B
				{
					for (int k = 0; k < aCols; ++k) // could use k < bRows
						result[i][j] += matrixA[i][k] * matrixB[k][j];
				}
			}

			return result;
		}


		static double[][] MatrixProductSimpleParallel(double[][] matrixA, double[][] matrixB)
		{
			int aRows = matrixA.Length; int aCols = matrixA[0].Length;
			int bRows = matrixB.Length; int bCols = matrixB[0].Length;
			if (aCols != bRows)
				throw new ArgumentException("Mismatching sizes");

			var result = CreateEmptyMatrix(aRows, bCols);

			Parallel.For(0, aRows, i =>
			{
				for (int j = 0; j < bCols; ++j) // each col of B
				{
					for (int k = 0; k < aCols; ++k) // could use k < bRows
						result[i][j] += matrixA[i][k] * matrixB[k][j];
				}
			});

			return result;
		}


		static double[][] MatrixProductTransposedParallel(double[][] matrixA, double[][] matrixB)
		{
			int aRows = matrixA.Length; int aCols = matrixA[0].Length;
			int bRows = matrixB.Length; int bCols = matrixB[0].Length;
			if (aCols != bRows)
				throw new ArgumentException("Mismatching sizes");

			var bTrans = CreateEmptyMatrix(bCols, bRows);
			Parallel.For(0, bRows, i =>
			{
				for (int j = 0; j < bCols; ++j)
				{
					bTrans[j][i] = matrixB[i][j];
				}
			});

			var result = CreateEmptyMatrix(aRows, bCols);

			Parallel.For(0, aRows, new ParallelOptions { MaxDegreeOfParallelism = 8 }, i =>
			{
				for (int j = 0; j < bCols; ++j) // each col of B
				{
					for (int k = 0; k < aCols; ++k) // could use k < bRows
						result[i][j] += matrixA[i][k] * bTrans[j][k];
				}
			});

			return result;
		}

		#endregion
	}
}
