using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Action委托demo
{
    public partial class Form1 : Form
    {
        //delegate void DisplayMessage(string message);//使用delegate方法 示例一
        //public delegate void ShowValue();//使用delegate方法 示例三
        delegate int del(int i);//示例四
        delegate void TestDelegate(string s);//示例七

        public Form1()
        {
            
            InitializeComponent();
            /*示例八 额外示例*/
            button9.Click += async (sender, e) =>
            {
                await ExampleMethodAsync();
                textBox1.Text += sender.ToString();
            };    
        }
        #region 示例一
        private void button1_Click(object sender, EventArgs e)
        {
           // DisplayMessage messageTarget; //使用delegate方法
            Action<string> messageTarget;//使用此方法只能有一个参数且没有返回值
            var str = textBox1.Text;
            if (str.Length > 1)
            {
                messageTarget = (s) => ShowWindowsMessage(s);
              
            }
            else
            {
                str = "null";
                messageTarget = delegate(string s) { Console.WriteLine(s); };
            }
            messageTarget(str);
        }
        private static void ShowWindowsMessage(string message)
        {
            MessageBox.Show(message);
        }
        #endregion

        #region 示例二
        private void button2_Click(object sender, EventArgs e)
        {
            List<string> names = new List<string>();
            names.Add("张三") ;
            names.Add("李四") ;
            names.Add("王五");
            names.Add("赵六");

            //foreach (var name in names)
            //{
            //    Print(name);
            //}
            names.ForEach(Print);
            names.ForEach(s => Print(s));
        }

        private static void Print(string s)
        {
            Console.WriteLine(s);
        }
        #endregion

        #region 示例三
        private void button3_Click(object sender, EventArgs e)
        {
          
            Name testName = new Name("张三");
           // Action showMethod1 = testName.DisplayToWindow;
            Action showMethod2 = () => testName.DisplayToWindow();
            showMethod2();
        }
        #endregion

        #region 示例四 Lambda表达式
        private void button4_Click(object sender, EventArgs e)
        {
            del myDelegate = x => x * x;
            Console.WriteLine(myDelegate); //输出变量的类型
            Console.WriteLine(myDelegate(5));//输出25
        }
        #endregion

        #region 示例五 表达式树
        private void button5_Click(object sender, EventArgs e)
        {
            Expression<del> myET = x => x * x;

            Console.WriteLine(myET.Compile()(5));
        }
        #endregion

        #region 示例六 Lambda表达式
        private void button6_Click(object sender, EventArgs e)
        {
            //使用LambdaExpression构建表达式树
            Expression<Func<int, int, int, int>> expr = (x, y, z) => (x + y) / z;
            Console.WriteLine(expr.Compile()(1, 2, 3));
            //使用LambdaExpression构建可执行的代码
            Func<int, int, int, int> fun = (x, y, z) => (x + y) / z;
            Console.WriteLine(fun(1, 2, 3));
            //动态构建表达式树
            ParameterExpression pe1 = Expression.Parameter(typeof(int), "x");
            ParameterExpression pe2 = Expression.Parameter(typeof(int), "y");
            ParameterExpression pe3 = Expression.Parameter(typeof(int), "z");
            var body = Expression.Divide(Expression.Add(pe1, pe2), pe3);//先创建一个表达式树 加法，再创建一个表达式树除法，作为一个抽象型的 （a+b）/c的形式
            var w = Expression.Lambda<Func<int, int, int, int>>(body, new ParameterExpression[] { pe1, pe2, pe3 });//根据body生成一个表达式树，并填入参数
            Console.WriteLine(w.Compile()(1, 2, 3));

            List<Entity> list = new List<Entity> { new Entity { Id1 = 1 }, new Entity { Id1 = 2 }, new Entity { Id1 = 3 } };
            var d = list.AsQueryable().WhereIn(o => o.Id1, new int[] { 1, 2,3 });
            d.ToList().ForEach(o =>
            {
                Console.WriteLine(o.Id1);
            });
        }
        #endregion

        #region 示例七
        private void button7_Click(object sender, EventArgs e)
        {
            TestDelegate myDel = n =>
            { 
                string s = n + " " + "World"; 
                Console.WriteLine(s); 
            };
            myDel("Hello");
        }
        #endregion

        #region 示例八 异步
        private async void button8_Click(object sender, EventArgs e)
        {
            await ExampleMethodAsync();
            textBox1.Text = sender.ToString();
        }
        async Task ExampleMethodAsync()
        {
            // The following line simulates a task-returning asynchronous process.
            await Task.Delay(1000);
        }
        #endregion

        private void button10_Click(object sender, EventArgs e)
        {
            int[] numbers = { 5, 4, 1, 3, 9, 8, 6, 7, 2, 0 };
            var oddNumbers = numbers.Where(x => x > 5);
            var result = oddNumbers.TakeWhile((x, index) => x >= index);
        }


    }

    public class Name
    {
       
        private string instanceName = "";
        public Name(string name)
        {
            this.instanceName = name;
        }

        public void DisplayToConsole()
        {
            Console.WriteLine(this.instanceName);
        }
        public void DisplayToWindow()
        {
            MessageBox.Show(this.instanceName);
        }

    }

    //示例六
    public class Entity
    {
        public Object Id;
        public int Id1;
        public string Name { get; set; }
    }
    public static class cc
    {
        public static IQueryable<T> WhereIn<T, TValue>(this IQueryable<T> query, Expression<Func<T, TValue>> obj, IEnumerable<TValue> values)
        {
            return query.Where(BuildContainsExpression(obj, values));
        }
        private static Expression<Func<TElement, bool>> BuildContainsExpression<TElement, TValue>(Expression<Func<TElement, TValue>> valueSelector, IEnumerable<TValue> values)
        {
            if (null == valueSelector)
            {
                throw new ArgumentNullException("valueSelector");
            }
            if (null == values)
            {
                throw new ArgumentNullException("values");
            }
            var p = valueSelector.Parameters.Single();
            if (!values.Any()) return e => false;
            var equals = values.Select(value => (Expression)Expression.Equal(valueSelector.Body, Expression.Constant(value, typeof(TValue))));
            var body = equals.Aggregate(Expression.Or);
            return Expression.Lambda<Func<TElement, bool>>(body, p);
        }
    }
}
