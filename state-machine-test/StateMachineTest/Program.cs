using System;
using jersmart;

namespace StateMachineTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press A to attack, R to retreat, D to kill the player, B to finish retreating.");

            var sm = new StateMachine<States, Triggers>(States.Patrolling);
            sm.AddTransition(States.Patrolling, Triggers.EnemyDetected, States.Attacking)
                .AddTransition(States.Attacking, Triggers.EnemyOutOfRange, States.Retreating)
                .AddTransition(States.Retreating, Triggers.ReturnedToPatrollPosition, States.Patrolling)
                .AddTransition(States.Attacking, Triggers.EnemyDied, States.Retreating);
            sm.AddAction(States.Patrolling, () => {
                Console.WriteLine("I'm patrolling");
            });
            sm.AddAction(States.Attacking, () => {
                Console.WriteLine("I'm attacking");
            });
            sm.AddAction(States.Retreating, () => {
                Console.WriteLine("I'm retreating");
            });

            var done = false;
            do
            {
                while (!Console.KeyAvailable)
                {
                    sm.Act();
                    System.Threading.Thread.Sleep(1000);
                }

                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.A && sm.CurrentState == States.Patrolling)
                {
                    sm.Transition(Triggers.EnemyDetected);
                }
                else if (key == ConsoleKey.R && sm.CurrentState == States.Attacking)
                {
                    sm.Transition(Triggers.EnemyOutOfRange);
                }
                else if (key == ConsoleKey.D && sm.CurrentState == States.Attacking)
                {
                    sm.Transition(Triggers.EnemyDied);
                }
                else if (key == ConsoleKey.B && sm.CurrentState == States.Retreating)
                {
                    sm.Transition(Triggers.ReturnedToPatrollPosition);
                }
                else if (key == ConsoleKey.Escape)
                {
                    done = true;
                }
            } while (!done);
        }

        private enum States
        {
            Patrolling,
            Attacking,
            Retreating
        }

        private enum Triggers
        {
            EnemyDetected,
            EnemyOutOfRange,
            EnemyDied,
            ReturnedToPatrollPosition
        }

    }

    
}
