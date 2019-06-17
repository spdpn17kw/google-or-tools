using System;
using System.Collections.Generic;
using System.IO;
using Google.OrTools.LinearSolver;
using Google.OrTools.Sat;

namespace spd
{
       class Job
    {
        public int id { get; set; }
        public int r { get; set; }
        public int p { get; set; }
        public int q { get; set; }
    }
    class Program
    {
        private static void RPQ_Solution(List<Job> Jobs)
        {
            Solver solver = Solver.CreateSolver("SimpleMipProgram","CBC_MIXED_INTEGER_PROGRAMMING");

            int variablesMaxValue = 0;
            foreach(var job in Jobs)
            {
                variablesMaxValue += job.r + job.p + job.q;
            }

            var alfas = solver.MakeIntVarMatrix(Jobs.Count, Jobs.Count, 0, 1); 

            var starts = solver.MakeIntVarArray(Jobs.Count, 0, variablesMaxValue);

            var cmax = solver.MakeIntVar(0, variablesMaxValue, "cmax");

            

            foreach(var job in Jobs)
            {
                solver.Add(starts[job.id] >= job.r);
            }

            foreach (var job in Jobs)
            {
                solver.Add(cmax >= starts[job.id] + job.p + job.q);
            }

            for(int i=0; i<Jobs.Count; i++)
            {
                for(int j = i+1; j< Jobs.Count; j++)
                {
                    var job1 = Jobs[i];
                    var job2 = Jobs[j];
                    solver.Add(starts[job1.id] + job1.p <= starts[job2.id] + alfas[job1.id, job2.id] * variablesMaxValue);
                    solver.Add(starts[job2.id] + job2.p <= starts[job1.id] + alfas[job2.id, job1.id] * variablesMaxValue);
                    solver.Add(alfas[job1.id, job2.id] + alfas[job2.id, job1.id] == 1);
                }
            }
            solver.Minimize(cmax);
            Solver.ResultStatus resultStatus = solver.Solve();
            if (resultStatus != Solver.ResultStatus.OPTIMAL)
            {
                Console.WriteLine("Solve nie znalazl optymalnego rozwiazania");
            }
            Console.WriteLine("Object_value = " + solver.Objective().Value());
        }

        private static void RPQ_Solution_CP(List<Job> Jobs)
        {
            CpModel model = new CpModel();

            IntVar[,] alfas = new IntVar[Jobs.Count, Jobs.Count];

          

            int variablesMaxValue = 0;
            foreach (var job in Jobs)
            {
                variablesMaxValue += job.r + job.p + job.q;
            }

            for (int i = 0; i < Jobs.Count; i++)
            {
                for (int j = 0; j < Jobs.Count; j++)
                {
                    alfas[i, j] = model.NewIntVar(0, variablesMaxValue, $"{i}g{j}");
                }
            }

            IntVar[] starts = new IntVar[Jobs.Count];
            for(int i=0; i<Jobs.Count; i++)
            {
                starts[i] = model.NewIntVar(0, variablesMaxValue, $"starts{i}");
            }

            

            var cmax = model.NewIntVar(0, variablesMaxValue, "cmax");

            foreach (var job in Jobs)
            {
                model.Add(starts[job.id] >= job.r);
            }

            foreach (var job in Jobs)
            {
                model.Add(cmax >= starts[job.id] + job.p + job.q);
            }

            for (int i = 0; i < Jobs.Count; i++)
            {
                for (int j = i + 1; j < Jobs.Count; j++)
                {
                    var job1 = Jobs[i];
                    var job2 = Jobs[j];
                    model.Add(starts[job1.id] + job1.p <= starts[job2.id] + alfas[job1.id, job2.id] * variablesMaxValue);
                    model.Add(starts[job2.id] + job2.p <= starts[job1.id] + alfas[job2.id, job1.id] * variablesMaxValue);
                    model.Add(alfas[job1.id, job2.id] + alfas[job2.id, job1.id] == 1);
                }
            }
            model.Minimize(cmax);

            CpSolver solver = new CpSolver();

            var resultStatus = solver.Solve(model);
            if (resultStatus != CpSolverStatus.Optimal)
            {
                Console.WriteLine("Solve nie znalazl optymalnego rozwiazania");
            }
            Console.WriteLine("Object_value = " + solver.Value(cmax));
        }

        public static int HowManyTasks;

        static void Main(string[] args)
        {
            string[] text = File.ReadAllLines("examples/dotnet/in24.txt"); 
            string dataStart = text[0];
            NumberOfJobs(dataStart);

            List<Job> FinallyTasks = new List<Job>();
            List<Job> Tasks = new List<Job>();
            LoadDataIntoTasks(Tasks, text);

           foreach(var element in Tasks)
            {
                Console.WriteLine(element.id);
            }
            RPQ_Solution_CP(Tasks);
			RPQ_Solution(Tasks);
             
        }


        static void NumberOfJobs(string dataStart)
        {
            string[] result = dataStart.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            HowManyTasks = Convert.ToInt32(result[0]);
        }
        static void LoadDataIntoTasks(List<Job> Tasks, string[] text)
        {
            string[] result;
            for (int i = 1; i <= HowManyTasks; i++)
            {
                result = text[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                Tasks.Add(new Job { id = i-1, r = Convert.ToInt32(result[0]), p = Convert.ToInt32(result[1]), q = Convert.ToInt32(result[2]) });
            }
        }

    }
}