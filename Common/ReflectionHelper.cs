using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Leclair.Stardew.Common;

internal static class ReflectionHelper {

	private static string MakeAccessorName(string prefix, MemberInfo field) {
		string assembly = Assembly.GetExecutingAssembly().GetName().Name ?? "Unknown";
		return $"{assembly}_{prefix}_{field.DeclaringType?.Name}_{field.Name}";
	}

	#region Fields

	private static readonly Dictionary<FieldInfo, Delegate> FieldGetters = new();
	private static readonly Dictionary<FieldInfo, Delegate> FieldSetters = new();

	#region Static Fields

	/// <summary>
	/// Use <see cref="System.Reflection.Emit"/> to create a dynamic method
	/// for reading the value of a static field. This is much more efficient
	/// than calling <see cref="FieldInfo.GetValue(object?)"/>.
	/// </summary>
	/// <typeparam name="TValue">The return type of the field.</typeparam>
	/// <param name="field">The <see cref="FieldInfo"/> to access.</param>
	/// <returns>A function for reading the value.</returns>
	/// <exception cref="ArgumentNullException">If the field is null</exception>
	/// <exception cref="ArgumentException">If the field is not static</exception>
	/// <exception cref="InvalidCastException">If the provided <typeparamref name="TValue"/> is not the field's type</exception>
	internal static Func<TValue> CreateGetter<TValue>(this FieldInfo field) {
		if (field is null)
			throw new ArgumentNullException(nameof(field));
		if (!field.FieldType.IsAssignableTo(typeof(TValue)))
			throw new InvalidCastException($"{typeof(TValue)} is not assignable from field type {field.FieldType}");
		if (!field.IsStatic)
			throw new ArgumentException("field is not static");

		if (!FieldGetters.TryGetValue(field, out var getter)) {
			DynamicMethod dm = new(MakeAccessorName("Get", field), typeof(TValue), null, true);

			var generator = dm.GetILGenerator();
			generator.Emit(OpCodes.Ldsfld, field);
			generator.Emit(OpCodes.Ret);

			getter = dm.CreateDelegate(typeof(Func<TValue>));
			FieldGetters[field] = getter;
		}

		return (Func<TValue>) getter;
	}

	/// <summary>
	/// Use <see cref="System.Reflection.Emit"/> to create a dynamic method
	/// for writing a value to a static field. This is much more efficient
	/// than calling <see cref="FieldInfo.SetValue(object?, object?)"/>.
	/// </summary>
	/// <typeparam name="TValue">The type of the field.</typeparam>
	/// <param name="field">The <see cref="FieldInfo"/> to access.</param>
	/// <returns>A function for writing the value.</returns>
	/// <exception cref="ArgumentNullException">If either argument is null</exception>
	/// <exception cref="ArgumentException">If the field is not static</exception>
	/// <exception cref="InvalidCastException">If the provided <typeparamref name="TValue"/> is not the field's type</exception>
	internal static Action<TValue> CreateSetter<TValue>(this FieldInfo field) {
		if (field is null)
			throw new ArgumentNullException(nameof(field));
		if (!field.FieldType.IsAssignableTo(typeof(TValue)))
			throw new InvalidCastException($"{typeof(TValue)} is not assignable from field type {field.FieldType}");
		if (!field.IsStatic)
			throw new ArgumentException("field is not static");

		if (!FieldSetters.TryGetValue(field, out var setter)) {
			DynamicMethod dm = new(MakeAccessorName("Set", field), null, [typeof(TValue)], true);

			var generator = dm.GetILGenerator();
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Stsfld, field);
			generator.Emit(OpCodes.Ret);

			setter = dm.CreateDelegate(typeof(Action<TValue>));
			FieldSetters[field] = setter;
		}

		return (Action<TValue>) setter;
	}

	#endregion

	#region Instance Fields

	/// <summary>
	/// Use <see cref="System.Reflection.Emit"/> to create a dynamic method
	/// for reading the value of a field on an object. This is much more
	/// efficient than calling <see cref="FieldInfo.GetValue(object?)"/>.
	/// </summary>
	/// <typeparam name="TOwner">The type of object that owns the field.</typeparam>
	/// <typeparam name="TValue">The type of the field.</typeparam>
	/// <param name="field">The <see cref="FieldInfo"/> to access.</param>
	/// <returns>A function for reading the value.</returns>
	/// <exception cref="ArgumentNullException">If the field is null</exception>
	/// <exception cref="ArgumentException">If the field is not static</exception>
	/// <exception cref="InvalidCastException">If the provided <typeparamref name="TOwner"/>
	/// or <typeparamref name="TValue"/> do not match the field.</exception>
	internal static Func<TOwner, TValue> CreateGetter<TOwner, TValue>(this FieldInfo field) {
		if (field is null || field.DeclaringType is null)
			throw new ArgumentNullException(nameof(field));
		if (typeof(TOwner) != typeof(object) && !field.DeclaringType.IsAssignableFrom(typeof(TOwner)))
			throw new InvalidCastException($"{typeof(TOwner)} is not assignable to declaring type {field.DeclaringType}");
		if (!field.FieldType.IsAssignableTo(typeof(TValue)))
			throw new InvalidCastException($"{typeof(TValue)} is not assignable from field type {field.FieldType}");
		if (field.IsStatic)
			throw new ArgumentException("field is static");

		if (!FieldGetters.TryGetValue(field, out var getter)) {
			DynamicMethod dm = new(MakeAccessorName("Get", field), typeof(TValue), [typeof(TOwner)], true);

			var generator = dm.GetILGenerator();
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldfld, field);
			generator.Emit(OpCodes.Ret);

			getter = dm.CreateDelegate(typeof(Func<TOwner, TValue>));
			FieldGetters[field] = getter;
		}

		return (Func<TOwner, TValue>) getter;
	}

	/// <summary>
	/// Use <see cref="System.Reflection.Emit"/> to create a dynamic method
	/// for writing a value to a field on an object. This is much more efficient
	/// than calling <see cref="FieldInfo.SetValue(object?, object?)"/>.
	/// </summary>
	/// <typeparam name="TOwner">The type of object that owns the field.</typeparam>
	/// <typeparam name="TValue">The type of the field.</typeparam>
	/// <param name="field">The <see cref="FieldInfo"/> to access.</param>
	/// <returns>A function for setting the value.</returns>
	/// <exception cref="ArgumentNullException"> If the field is null</exception>
	/// <exception cref="ArgumentException">If the field is not static</exception>
	/// <exception cref="InvalidCastException">If the provided <typeparamref name="TOwner"/>
	/// or <typeparamref name="TValue"/> do not match the field.</exception>
	internal static Action<TOwner, TValue> CreateSetter<TOwner, TValue>(this FieldInfo field) {
		if (field is null || field.DeclaringType is null)
			throw new ArgumentNullException(nameof(field));
		if (typeof(TOwner) != typeof(object) && !field.DeclaringType.IsAssignableFrom(typeof(TOwner)))
			throw new InvalidCastException($"{typeof(TOwner)} is not assignable to declaring type {field.DeclaringType}");
		if (!field.FieldType.IsAssignableTo(typeof(TValue)))
			throw new InvalidCastException($"{typeof(TValue)} is not assignable from field type {field.FieldType}");
		if (field.IsStatic)
			throw new ArgumentException("field is static");

		if (!FieldSetters.TryGetValue(field, out var setter)) {
			DynamicMethod dm = new(MakeAccessorName("Set", field), null, [typeof(TOwner), typeof(TValue)], true);

			var generator = dm.GetILGenerator();
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldarg_1);
			generator.Emit(OpCodes.Stfld, field);
			generator.Emit(OpCodes.Ret);

			setter = dm.CreateDelegate(typeof(Action<TOwner, TValue>));
			FieldSetters[field] = setter;
		}

		return (Action<TOwner, TValue>) setter;
	}

	#endregion

	#endregion

	#region Methods

	private static readonly Dictionary<MethodInfo, Delegate> MethodCallers = new();

	internal static Delegate CreateFuncInner(this MethodInfo method, Type ownerType, Type resultType, params Type[] types) {
		if (method is null || method.DeclaringType is null)
			throw new ArgumentNullException(nameof(method));
		if (!resultType.IsAssignableFrom(method.ReturnType))
			throw new InvalidCastException($"{resultType} is not assignable from return type {method.ReturnType}");
		if (ownerType != typeof(object) && !method.DeclaringType.IsAssignableFrom(ownerType))
			throw new InvalidCastException($"{ownerType} is not assignable to declaring type {method.DeclaringType}");
		if (method.IsStatic)
			throw new ArgumentException("method is static");

		var parms = method.GetParameters();
		if (parms.Length != types.Length)
			throw new ArgumentException("incorrect parameter count");

		for (int i = 0; i < types.Length; i++) {
			if (!parms[i].ParameterType.IsValueType && types[i] == typeof(object))
				continue;
			if (!parms[i].ParameterType.IsAssignableFrom(types[i]))
				throw new ArgumentException($"Parameter type mismatch at index {i}. Expected: {parms[i].ParameterType}, Actual: {types[i]}");
		}

		if (!MethodCallers.TryGetValue(method, out var caller)) {
			Type[] finalTypes = [ownerType, .. types];
			var delegateType = Expression.GetFuncType([.. finalTypes, resultType]);

			DynamicMethod dm = new(MakeAccessorName("Call", method), resultType, finalTypes, true);

			var generator = dm.GetILGenerator();

			if (ownerType.IsValueType)
				generator.Emit(OpCodes.Ldarga_S, (byte) 0);
			else
				generator.Emit(OpCodes.Ldarg_0);

			for (byte i = 1; i <= parms.Length; i++)
				generator.Emit(OpCodes.Ldarg_S, i);

			generator.Emit(OpCodes.Call, method);
			generator.Emit(OpCodes.Ret);

			caller = dm.CreateDelegate(delegateType);
			MethodCallers[method] = caller;
		}

		return caller;
	}


	internal static Func<TOwner, TResult> CreateFunc<TOwner, TResult>(this MethodInfo method) {
		return (Func<TOwner, TResult>) CreateFuncInner(method, typeof(TOwner), typeof(TResult));
	}

	internal static Func<TOwner, TArg1, TResult> CreateFunc<TOwner, TArg1, TResult>(this MethodInfo method) {
		return (Func<TOwner, TArg1, TResult>) CreateFuncInner(method, typeof(TOwner), typeof(TResult), typeof(TArg1));
	}

	internal static Func<TOwner, TArg1, TArg2, TResult> CreateFunc<TOwner, TArg1, TArg2, TResult>(this MethodInfo method) {
		return (Func<TOwner, TArg1, TArg2, TResult>) CreateFuncInner(method, typeof(TOwner), typeof(TResult), typeof(TArg1), typeof(TArg2));
	}

	internal static Delegate CreateActionInner(this MethodInfo method, Type ownerType, params Type[] types) {
		if (method is null || method.DeclaringType is null)
			throw new ArgumentNullException(nameof(method));
		if (ownerType != typeof(object) && !method.DeclaringType.IsAssignableFrom(ownerType))
			throw new InvalidCastException($"{ownerType} is not same as parent type {method.ReflectedType}");
		if (method.IsStatic)
			throw new ArgumentException("method is static");

		var parms = method.GetParameters();
		if (parms.Length != types.Length)
			throw new ArgumentException("incorrect parameter count");

		for (int i = 0; i < types.Length; i++) {
			if (!parms[i].ParameterType.IsValueType && types[i] == typeof(object))
				continue;
			if (!parms[i].ParameterType.IsAssignableFrom(types[i]))
				throw new ArgumentException($"Parameter type mismatch at index {i}. Expected: {parms[i].ParameterType}, Actual: {types[i]}");
		}

		if (true || !MethodCallers.TryGetValue(method, out var caller)) {
			Type[] finalTypes = [ownerType, .. types];
			var delegateType = Expression.GetActionType(finalTypes);

			DynamicMethod dm = new(MakeAccessorName("Call", method), null, finalTypes, true);

			var generator = dm.GetILGenerator();

			if (ownerType.IsValueType)
				generator.Emit(OpCodes.Ldarga_S, (byte) 0);
			else
				generator.Emit(OpCodes.Ldarg_0);

			for (byte i = 1; i <= parms.Length; i++)
				generator.Emit(OpCodes.Ldarg_S, i);

			generator.Emit(OpCodes.Call, method);
			generator.Emit(OpCodes.Ret);

			caller = dm.CreateDelegate(delegateType);
			MethodCallers[method] = caller;
		}

		return caller;
	}


	internal static Action<TOwner> CreateAction<TOwner>(this MethodInfo method) {
		return (Action<TOwner>) CreateActionInner(method, typeof(TOwner));
	}

	internal static Action<TOwner, TArg1> CreateAction<TOwner, TArg1>(this MethodInfo method) {
		return (Action<TOwner, TArg1>) CreateActionInner(method, typeof(TOwner), typeof(TArg1));
	}

	internal static Action<TOwner, TArg1, TArg2> CreateAction<TOwner, TArg1, TArg2>(this MethodInfo method) {
		return (Action<TOwner, TArg1, TArg2>) CreateActionInner(method, typeof(TOwner), typeof(TArg1), typeof(TArg2));
	}

	internal static Action<TOwner, TArg1, TArg2, TArg3> CreateAction<TOwner, TArg1, TArg2, TArg3>(this MethodInfo method) {
		return (Action<TOwner, TArg1, TArg2, TArg3>) CreateActionInner(method, typeof(TOwner), typeof(TArg1), typeof(TArg2), typeof(TArg3));
	}

	#endregion

	#region Properties

	private static readonly Dictionary<PropertyInfo, Delegate> PropertyGetters = new();
	private static readonly Dictionary<PropertyInfo, Delegate> PropertySetters = new();

	#region Static Properties

	/// <summary>
	/// Use <see cref="System.Reflection.Emit"/> to create a dynamic method
	/// for reading the value from a static property. This is much more efficient
	/// than calling <see cref="PropertyInfo.GetValue(object?)"/>.
	/// </summary>
	/// <typeparam name="TValue">The return type of the property.</typeparam>
	/// <param name="property">The <see cref="PropertyInfo"/> to access.</param>
	/// <returns>A function for reading the value.</returns>
	/// <exception cref="ArgumentNullException">If the property is null</exception>
	/// <exception cref="ArgumentException">If the property is not static</exception>
	/// <exception cref="InvalidCastException">If the provided <typeparamref name="TValue"/> is not the property's type</exception>
	internal static Func<TValue> CreateGetter<TValue>(this PropertyInfo property) {
		if (property is null)
			throw new ArgumentNullException(nameof(property));
		if (typeof(TValue) != property.PropertyType)
			throw new InvalidCastException($"{typeof(TValue)} is not same as property type {property.PropertyType}");

		if (!PropertyGetters.TryGetValue(property, out var getter)) {
			var getMethod = property.GetGetMethod(nonPublic: true) ?? throw new ArgumentNullException("property has no getter");
			if (!getMethod.IsStatic)
				throw new ArgumentException("property is not static");

			DynamicMethod dm = new(MakeAccessorName("Get", property), typeof(TValue), null, true);

			var generator = dm.GetILGenerator();
			generator.Emit(OpCodes.Call, getMethod);
			generator.Emit(OpCodes.Ret);

			getter = dm.CreateDelegate(typeof(Func<TValue>));
			PropertyGetters[property] = getter;
		}

		return (Func<TValue>) getter;
	}

	/// <summary>
	/// Use <see cref="System.Reflection.Emit"/> to create a dynamic method
	/// for writing a value to a static property. This is much more efficient
	/// than calling <see cref="PropertyInfo.SetValue(object?, object?)"/>.
	/// </summary>
	/// <typeparam name="TValue">The type of the property.</typeparam>
	/// <param name="property">The <see cref="PropertyInfo"/> to access.</param>
	/// <returns>A function for writing the value.</returns>
	/// <exception cref="ArgumentNullException">If the property is null</exception>
	/// <exception cref="ArgumentException">If the property is not static</exception>
	/// <exception cref="InvalidCastException">If the provided <typeparamref name="TValue"/> is not the property's type</exception>
	internal static Action<TValue> CreateSetter<TValue>(this PropertyInfo property) {
		if (property is null)
			throw new ArgumentNullException(nameof(property));
		if (typeof(TValue) != property.PropertyType)
			throw new InvalidCastException($"{typeof(TValue)} is not same as property type {property.PropertyType}");

		if (!PropertySetters.TryGetValue(property, out var setter)) {
			var setMethod = property.GetSetMethod(nonPublic: true) ?? throw new ArgumentNullException("property has no setter");
			if (!setMethod.IsStatic)
				throw new ArgumentException("property is not static");

			DynamicMethod dm = new(MakeAccessorName("Set", property), typeof(TValue), null, true);

			var generator = dm.GetILGenerator();
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Call, setMethod);
			generator.Emit(OpCodes.Ret);

			setter = dm.CreateDelegate(typeof(Action<TValue>));
			PropertyGetters[property] = setter;
		}

		return (Action<TValue>) setter;
	}

	#endregion

	#region Instance Properties

	/// <summary>
	/// Use <see cref="System.Reflection.Emit"/> to create a dynamic method
	/// for reading the value from an instance property. This is much more
	/// efficient than calling <see cref="PropertyInfo.GetValue(object?)"/>.
	/// </summary>
	/// <typeparam name="TOwner">The type of object that owns the property.</typeparam>
	/// <typeparam name="TValue">The type of the property.</typeparam>
	/// <param name="property">The <see cref="PropertyInfo"/> to access.</param>
	/// <returns>A function for reading the value.</returns>
	/// <exception cref="ArgumentNullException">If the property is null</exception>
	/// <exception cref="ArgumentException">If the property is not static</exception>
	/// <exception cref="InvalidCastException">If the provided <typeparamref name="T"/> is not the property's type</exception>
	internal static Func<TOwner, TValue> CreateGetter<TOwner, TValue>(this PropertyInfo property) {
		if (property is null)
			throw new ArgumentNullException(nameof(property));
		if (typeof(TValue) != property.PropertyType)
			throw new InvalidCastException($"{typeof(TValue)} is not same as property type {property.PropertyType}");
		if (typeof(TOwner) != property.DeclaringType)
			throw new InvalidCastException($"{typeof(TOwner)} is not the same as declaring type {property.DeclaringType}");

		if (!PropertyGetters.TryGetValue(property, out var getter)) {
			var getMethod = property.GetGetMethod(nonPublic: true) ?? throw new ArgumentNullException("property has no getter");
			if (getMethod.IsStatic)
				throw new ArgumentException("property is static");

			DynamicMethod dm = new(MakeAccessorName("Get", property), typeof(TValue), [typeof(TOwner)], true);

			var generator = dm.GetILGenerator();
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Call, getMethod);
			generator.Emit(OpCodes.Ret);

			getter = dm.CreateDelegate(typeof(Func<TOwner, TValue>));
			PropertyGetters[property] = getter;
		}

		return (Func<TOwner, TValue>) getter;
	}

	/// <summary>
	/// Use <see cref="System.Reflection.Emit"/> to create a dynamic method
	/// for writing a value to an instance property. This is much more efficient
	/// than calling <see cref="PropertyInfo.SetValue(object?, object?)"/>.
	/// </summary>
	/// <typeparam name="TOwner">The type of object that owns the property.</typeparam>
	/// <typeparam name="TValue">The type of the property.</typeparam>
	/// <param name="property">The <see cref="PropertyInfo"/> to access.</param>
	/// <returns>A function for writing the value.</returns>
	/// <exception cref="ArgumentNullException">If the property is null</exception>
	/// <exception cref="ArgumentException">If the property is not static</exception>
	/// <exception cref="InvalidCastException">If the provided <typeparamref name="T"/> is not the property's type</exception>
	internal static Action<TOwner, TValue> CreateSetter<TOwner, TValue>(this PropertyInfo property) {
		if (property is null)
			throw new ArgumentNullException(nameof(property));
		if (typeof(TValue) != property.PropertyType)
			throw new InvalidCastException($"{typeof(TValue)} is not same as property type {property.PropertyType}");
		if (typeof(TOwner) != property.DeclaringType)
			throw new InvalidCastException($"{typeof(TOwner)} is not the same as declaring type {property.DeclaringType}");

		if (!PropertySetters.TryGetValue(property, out var setter)) {
			var setMethod = property.GetSetMethod(nonPublic: true) ?? throw new ArgumentNullException("property has no setter");
			if (setMethod.IsStatic)
				throw new ArgumentException("property is static");

			DynamicMethod dm = new(MakeAccessorName("Set", property), null, [typeof(TOwner), typeof(TValue)], true);

			var generator = dm.GetILGenerator();
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldarg_1);
			generator.Emit(OpCodes.Call, setMethod);
			generator.Emit(OpCodes.Ret);

			setter = dm.CreateDelegate(typeof(Action<TOwner, TValue>));
			PropertyGetters[property] = setter;
		}

		return (Action<TOwner, TValue>) setter;
	}

	#endregion

	#endregion

}
