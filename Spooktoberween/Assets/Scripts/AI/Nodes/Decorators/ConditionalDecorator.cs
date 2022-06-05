
public class ConditionalDecorator<T> : DecoratorNode
{
    public BehaviorProperty<T> Property;
    public BehaviorProperty<T> ReferenceProperty;
    public T ReferenceValue;

    public delegate bool ComparisonDelegate(in T A, in T B);
    public ComparisonDelegate ComparisionOperation;

    protected override ENodeStatus Tick(float DeltaSeconds)
    {

        bool bCanExecute = false;
        if(Property != null && ComparisionOperation != null)
        {
            T Reference = ReferenceProperty == null ? ReferenceValue : ReferenceProperty.Value;

            bCanExecute = ComparisionOperation(Property.Value, Reference);
        }

        if(bCanExecute)
        {
            return base.Tick(DeltaSeconds);
        }
        else
        {
            Child.Cancel();
            return ENodeStatus.NotRunning;
        }
    }
}
