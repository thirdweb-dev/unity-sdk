using System;
using System.Threading.Tasks;

namespace WalletConnectSharp.Sign.Models.Engine
{
    /// <summary>
    /// An interface that represents an action that has an acknowledgement step. The interface
    /// includes a single Task that resolves when the acknowledgement occurs.
    /// </summary>
    public interface IAcknowledgement
    {
        /// <summary>
        /// A task that will return when the acknowledgement occurs. 
        /// </summary>
        Task Acknowledged();

        /// <summary>
        /// Convert an action to an <see cref="IAcknowledgement"/> This will run the given <see cref="Action"/> and resolve
        /// the <see cref="IAcknowledgement.Acknowledged"/> task when the given <see cref="Action"/> completes
        /// </summary>
        /// <param name="ack">The acknowledgement action to run</param>
        /// <returns>A new <see cref="IAcknowledgement"/> interface that can be used to await the acknowledgement</returns>
        public static IAcknowledgement FromAction(Action ack)
        {
            return new ActionAcknowledgement(ack);
        }

        /// <summary>
        /// Convert a Task to an <see cref="IAcknowledgement"/> This will await the given <see cref="Task"/> and resolve
        /// the <see cref="IAcknowledgement.Acknowledged"/> task when the given <see cref="Task"/> completes
        /// </summary>
        /// <param name="task">The task that will resolve once acknowledgement occurs</param>
        /// <returns>A new <see cref="IAcknowledgement"/> interface that can be used to await the acknowledgement</returns>
        public static IAcknowledgement FromTask(Task task)
        {
            return new TaskAcknowledgement(task);
        }

        /// <summary>
        /// A class that implements <see cref="IAcknowledgement"/> that uses a backing <see cref="Action"/>
        /// This will run the given <see cref="Action"/> and resolve
        /// the <see cref="IAcknowledgement.Acknowledged"/> task when the given <see cref="Action"/> completes
        /// </summary>
        public class ActionAcknowledgement : IAcknowledgement
        {
            private Action _action;

            /// <summary>
            /// Create a new <see cref="IAcknowledgement.ActionAcknowledgement"/> with the given <see cref="Action"/>
            /// </summary>
            /// <param name="action">The action to perform for acknowledgement</param>
            public ActionAcknowledgement(Action action)
            {
                this._action = action;
            }
            
            /// <summary>
            /// A task that will return when the acknowledgement occurs. 
            /// </summary>
            public Task Acknowledged()
            {
                _action();
                return Task.CompletedTask;
            }
        }
        
        /// <summary>
        /// A class that implements <see cref="IAcknowledgement"/> that uses a backing <see cref="Task"/>
        /// This will await the given <see cref="Task"/> and resolve
        /// the <see cref="IAcknowledgement.Acknowledged"/> task when the given <see cref="Task"/> completes
        /// </summary>
        public class TaskAcknowledgement : IAcknowledgement
        {
            private Task _task;

            /// <summary>
            /// Create a new <see cref="IAcknowledgement.TaskAcknowledgement"/> with the given <see cref="Task"/>
            /// </summary>
            /// <param name="task">The <see cref="Task"/> that is awaiting acknowledgement</param>
            public TaskAcknowledgement(Task task)
            {
                this._task = task;
            }
            
            /// <summary>
            /// A task that will return when the acknowledgement occurs. 
            /// </summary>
            public async Task Acknowledged()
            {
                await _task;
            }
        }
    }
}
