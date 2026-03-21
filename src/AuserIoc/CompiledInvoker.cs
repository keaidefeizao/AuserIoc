using System.Collections.Concurrent;
using System.Reflection;
using System.Linq.Expressions;

namespace AuserIoc;

/// <summary>
/// 预编译委托调用器
/// 使用表达式树预编译工厂方法和构造函数调用，替代 DynamicInvoke 和 ConstructorInfo.Invoke 提升性能
/// </summary>
internal static class CompiledInvoker
{
    /// <summary>
    /// 预编译的无参数工厂方法委托
    /// </summary>
    private static readonly ConcurrentDictionary<MethodInfo, Func<object>> _compiledFactoryMethods = new();

    /// <summary>
    /// 预编译的有参数工厂方法委托
    /// </summary>
    private static readonly ConcurrentDictionary<MethodInfo, Func<object[], object>> _compiledFactoryMethodsWithParams = new();

    /// <summary>
    /// 预编译的构造函数委托
    /// </summary>
    private static readonly ConcurrentDictionary<ConstructorInfo, Func<object[], object>> _compiledConstructors = new();

    /// <summary>
    /// 获取或创建预编译的工厂方法调用器（无参数）
    /// </summary>
    internal static Func<object> GetOrAddFactoryMethod(MethodInfo method, Delegate factoryMethod)
    {
        return _compiledFactoryMethods.GetOrAdd(method, m =>
        {
            var factoryMethodInfo = factoryMethod.GetType().GetMethod("Invoke")!;

            // 为 Func<object> 类型的工厂方法创建直接调用
            if (factoryMethodInfo.ReturnType == typeof(object) && factoryMethodInfo.GetParameters().Length == 0)
            {
                return () => factoryMethod.DynamicInvoke(null)!;
            }

            // 为其他无参数工厂方法创建表达式树
            var callExpression = Expression.Call(
                Expression.Constant(factoryMethod),
                factoryMethodInfo
            );

            var lambda = Expression.Lambda<Func<object>>(
                Expression.Convert(callExpression, typeof(object))
            );

            return lambda.Compile();
        });
    }

    /// <summary>
    /// 获取或创建预编译的工厂方法调用器（有参数）
    /// </summary>
    internal static Func<object[], object> GetOrAddFactoryMethodWithParams(MethodInfo method, Delegate factoryMethod)
    {
        return _compiledFactoryMethodsWithParams.GetOrAdd(method, m =>
        {
            var factoryMethodInfo = factoryMethod.GetType().GetMethod("Invoke")!;
            var parameters = factoryMethodInfo.GetParameters();

            // 如果工厂方法接受 IIocContainer 参数
            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(IIocContainer))
            {
                // 这种情况需要特殊处理，在运行时解析容器
                return args =>
                {
                    var container = (IIocContainer)args[0];
                    var factoryMethodInfoLocal = factoryMethod.GetType().GetMethod("Invoke")!;
                    return factoryMethodInfoLocal.Invoke(factoryMethod, [container])!;
                };
            }

            // 为其他有参数工厂方法创建表达式树
            var argsParam = Expression.Parameter(typeof(object[]), "args");
            var paramExpressions = new Expression[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                var paramType = parameters[i].ParameterType;
                paramExpressions[i] = Expression.Convert(
                    Expression.ArrayIndex(argsParam, Expression.Constant(i)),
                    paramType
                );
            }

            var callExpression = Expression.Call(
                Expression.Constant(factoryMethod),
                factoryMethodInfo,
                paramExpressions
            );

            var lambda = Expression.Lambda<Func<object[], object>>(
                Expression.Convert(callExpression, typeof(object))
            );

            return lambda.Compile();
        });
    }

    /// <summary>
    /// 获取或创建预编译的构造函数调用器
    /// </summary>
    internal static Func<object[], object> GetOrAddConstructor(ConstructorInfo constructor)
    {
        return _compiledConstructors.GetOrAdd(constructor, c =>
        {
            var parameters = constructor.GetParameters();

            // 无参数构造函数：优化为直接调用
            if (parameters.Length == 0)
            {
                var newExpr = Expression.New(constructor);
                var lambda = Expression.Lambda<Func<object>>(
                    Expression.Convert(newExpr, typeof(object))
                );
                var compiled = lambda.Compile();
                // 包装为 Func<object[], object> 签名
                return args => compiled();
            }

            // 有参数构造函数：使用表达式树编译
            var argsParam = Expression.Parameter(typeof(object[]), "args");
            var paramExpressions = new Expression[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                var paramType = parameters[i].ParameterType;
                paramExpressions[i] = Expression.Convert(
                    Expression.ArrayIndex(argsParam, Expression.Constant(i)),
                    paramType
                );
            }

            var newExpression = Expression.New(constructor, paramExpressions);

            var ctorLambda = Expression.Lambda<Func<object[], object>>(
                Expression.Convert(newExpression, typeof(object)),
                argsParam
            );

            return ctorLambda.Compile();
        });
    }

    /// <summary>
    /// 清除缓存（用于测试或内存管理）
    /// </summary>
    internal static void ClearCache()
    {
        _compiledFactoryMethods.Clear();
        _compiledFactoryMethodsWithParams.Clear();
        _compiledConstructors.Clear();
    }
}
