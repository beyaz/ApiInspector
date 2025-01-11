namespace ApiInspector;

[Serializable]
public sealed class AssemblyReference
{
    public string Name { get; init; }

    public bool Equals(AssemblyReference other)
    {
        return Name == other?.Name;
    }

    public override string ToString()
    {
        return Name;
    }
}

[Serializable]
public sealed class TypeReference
{
    public AssemblyReference Assembly { get; init; }

    public string FullName { get; init; }

    public string Name { get; init; }

    public string NamespaceName { get; init; }

    public bool Equals(TypeReference other)
    {
        return Assembly.Equals(other?.Assembly) && FullName == other?.FullName;
    }

    public override string ToString()
    {
        return $"{FullName},{Assembly}";
    }
}

[Serializable]
public sealed class MethodReference
{
    public TypeReference DeclaringType { get; init; }

    public string FullNameWithoutReturnType { get; init; }

    public bool IsStatic { get; init; }

    public int MetadataToken { get; init; }

    public string Name { get; init; }

    public IReadOnlyList<ParameterReference> Parameters { get; init; }

    public bool Equals(MethodReference other)
    {
        if (DeclaringType is not null)
        {
            if (DeclaringType.Equals(other?.DeclaringType) == false)
            {
                return false;
            }
        }

        if (FullNameWithoutReturnType != other?.FullNameWithoutReturnType)
        {
            return false;
        }

        return true;
    }

    public override string ToString()
    {
        return $"{DeclaringType.FullName}::{FullNameWithoutReturnType}";
    }
}

[Serializable]
public sealed class ParameterReference
{
    public string Name { get; init; }

    public TypeReference ParameterType { get; init; }

    public override string ToString()
    {
        return $"{ParameterType} {Name}";
    }
}