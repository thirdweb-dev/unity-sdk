using System;
using System.Threading.Tasks;

namespace WalletConnectSharp.Sign.Models.Engine
{
    /// <summary>
    /// An interface that is inherits from <see cref="IAcknowledgement"/> but is specifically for
    /// Session Approval acknowledgement. This interface includes the Topic the new session exists in
    /// </summary>
    public interface IApprovedData : IAcknowledgement
    {
        /// <summary>
        /// The acknowledged session topic
        /// </summary>
        string Topic { get; }
        
        /// <summary>
        /// Convert an action to an <see cref="IApprovedData"/> This will run the given <see cref="Action"/> and resolve
        /// the <see cref="IAcknowledgement.Acknowledged"/> task when the given <see cref="Action"/> completes
        /// </summary>
        /// <param name="topic">The topic to store with this <see cref="IApprovedData"/></param>
        /// <param name="ack">The acknowledgement action to run</param>
        /// <returns>A new <see cref="IApprovedData"/> interface that can be used to await the session approval acknowledgement</returns>
        public static IApprovedData FromAction(string topic, Action ack)
        {
            return new ActionApprovedData(ack, topic);
        }

        /// <summary>
        /// Convert a Task to an <see cref="IAcknowledgement"/> This will await the given <see cref="Task"/> and resolve
        /// the <see cref="IAcknowledgement.Acknowledged"/> task when the given <see cref="Task"/> completes
        /// </summary>
        /// <param name="topic">The topic to store with this <see cref="IApprovedData"/></param>
        /// <param name="task">The task that will resolve once acknowledgement occurs</param>
        /// <returns>A new <see cref="IApprovedData"/> interface that can be used to await the session approval acknowledgement</returns>
        public static IApprovedData FromTask(string topic, Task task)
        {
            return new TaskApprovedData(task, topic);
        }

        /// <summary>
        /// A class that implements <see cref="IApprovedData"/> interface using <see cref="IAcknowledgement.ActionAcknowledgement"/>
        /// </summary>
        public class ActionApprovedData : ActionAcknowledgement, IApprovedData
        {
            /// <summary>
            /// Create a new <see cref="IApprovedData.ActionApprovedData"/> with the given <see cref="Action"/>
            /// and topic string
            /// </summary>
            /// <param name="action">The action to perform for acknowledgement</param>
            /// <param name="topic">The session topic to store as acknowledged or awaiting acknowledgement</param>
            public ActionApprovedData(Action action, string topic) : base(action)
            {
                Topic = topic;
            }

            /// <summary>
            /// The acknowledged session topic
            /// </summary>
            public string Topic { get; }
        }
        
        /// <summary>
        /// A class that implements <see cref="IApprovedData"/> interface using <see cref="IAcknowledgement.TaskAcknowledgement"/>
        /// </summary>
        public class TaskApprovedData : TaskAcknowledgement, IApprovedData
        {
            /// <summary>
            /// Create a new <see cref="IApprovedData.TaskApprovedData"/> with the given <see cref="Task"/>
            /// and topic string
            /// </summary>
            /// <param name="task">The <see cref="Task"/> that is awaiting acknowledgement</param>
            /// <param name="topic">The session topic to store as acknowledged or awaiting acknowledgement</param>
            public TaskApprovedData(Task task, string topic) : base(task)
            {
                Topic = topic;
            }

            /// <summary>
            /// The acknowledged session topic
            /// </summary>
            public string Topic { get; }
        }
    }
}
