using Compilify.Models;

namespace Compilify.Web.Models
{
    /// <summary>
    /// Represents the state of a user's workspace.</summary>
    public class WorkspaceState
    {
        /// <summary>
        /// The active project.</summary>
        public Project Project { get; set; }
    }
}