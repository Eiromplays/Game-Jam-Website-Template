namespace GameJam.Api.Results
{
    public class GameResult
    {
        /// <summary>
        /// Returns a flag indication whether it was successful.
        /// </summary>
        /// <value>True if it succeeded, otherwise false.</value>
        public bool Succeeded { get; protected set; }

        /// <summary>
        /// Returns a flag indicating whether the game was found or not.
        /// </summary>
        /// <value>True if the game was not found, otherwise false.</value>
        public bool IsNotFound { get; protected set; }

        /// <summary>
        /// Returns a flag indicating the user has submitted the maximum allowed games.
        /// </summary>
        /// <value>True if max submissions is reached, otherwise false.</value>
        public bool HasMaxSubmissions { get; protected set; }

        /// <summary>
        /// Returns a flag indicating if the max submissions value is set or valid
        /// </summary>
        /// <value>True if not valid/set, otherwise false.</value>
        public bool IsMaxSubmissionsNotSet { get; protected set; }

        /// <returns>A <see cref="GameResult"/> which represents that something went wrong.</returns>
        public static GameResult Failed { get; } = new GameResult();

        /// <returns>A <see cref="GameResult"/> which represents that something succeeded.</returns>
        public static GameResult Success { get; } = new GameResult { Succeeded = true };

        /// <returns>A <see cref="GameResult"/> which represents that it was unable to find the game.</returns>
        public static GameResult NotFound { get; } = new GameResult { IsNotFound = true };

        /// <returns>A <see cref="GameResult"/> which represents that the user has uploaded the maximum allowed games.</returns>
        public static GameResult MaxSubmissions { get; } = new GameResult { HasMaxSubmissions = true };

        /// <returns>A <see cref="GameResult"/> which represents that the Max submissions is not valid or not set.</returns>
        public static GameResult MaxSubmissionsNotSet { get; } = new GameResult { IsMaxSubmissionsNotSet = true };

        /// <summary>
        /// Converts the value of the current <see cref="GameResult"/> object to its equivalent string representation.
        /// </summary>
        /// <returns>A string representation of value of the current <see cref="GameResult"/> object.</returns>
        public override string ToString()
        {
            return IsNotFound ? "NotFound" :
                Succeeded ? "Succeeded" : "Failed";
        }
    }
}
