using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace AlvTime.Business;

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
        TResult success,
        Func<List<Error>, TResult> failure) =>
        IsSuccess ? success : failure(Errors);
}

public readonly struct Result<TValue>
{
    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsSuccess { get; }

    public readonly TValue Value;
    public List<Error> Errors { get; } = new();

    public Result(TValue value)
    {
        IsSuccess = true;
        Value = value;
    }

    private Result(List<Error> errors)
    {
        IsSuccess = false;
        Value = default;
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
        IsSuccess ? success(Value) : failure(Errors);
}
