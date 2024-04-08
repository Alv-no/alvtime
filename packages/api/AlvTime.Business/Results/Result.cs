namespace AlvTime.Business;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

public readonly struct Result
{
    public bool IsSuccess { get; }
    public List<Error> Errors { get; } = new();

    public Result()
    {
        IsSuccess = true;
    }

    private Result(List<Error> errors)
    {
        IsSuccess = false;
        Errors = errors;
    }

    private Result(Error error)
    {
        IsSuccess = false;
        Errors = new List<Error> { error };
    }

    public static implicit operator Result(List<Error> errors) => new(errors);

    public static implicit operator Result(Error error) => new(error);
    
    public TResult Match<TResult>(
        Func<TResult> success,
        Func<List<Error>, TResult> failure) =>
        IsSuccess ? success() : failure(Errors);
}

public readonly struct Result<TValue>
{
    [MemberNotNullWhen(true, nameof(_value))]
    public bool IsSuccess { get; }

    private readonly TValue? _value;
    public List<Error> Errors { get; } = new();

    public Result(TValue value)
    {
        IsSuccess = true;
        _value = value;
    }

    private Result(List<Error> errors)
    {
        IsSuccess = false;
        _value = default;
        Errors = errors;
    }

    private Result(Error error)
    {
        IsSuccess = false;
        Errors = new List<Error> { error };
    }

    public static implicit operator Result<TValue>(TValue value) => new(value);

    public static implicit operator Result<TValue>(List<Error> errors) => new(errors);

    public static implicit operator Result<TValue>(Error error) => new(error);

    public TResult Match<TResult>(
        Func<TValue, TResult> success,
        Func<List<Error>, TResult> failure) =>
        IsSuccess ? success(_value) : failure(Errors);
}
