namespace Virgil.CLI.Common.Handlers
{
    public abstract class CommandHandler<T>
    {
        public abstract int Handle(T command);
    }
}