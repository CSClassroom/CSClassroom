using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSC.CSClassroom.WebApp.ModelBinders.Question
{
	using System.Reflection;
	using Question = CSC.CSClassroom.Model.Exercises.Question;

	/// <summary>
	/// Binds a question model.
	/// </summary>
	public class QuestionModelBinder : IModelBinder
	{
		/// <summary>
		/// The model metadata provider.
		/// </summary>
		private IDictionary<Type, IModelBinder> _derivedModelBinders;

		/// <summary>
		/// The model metadata provider.
		/// </summary>
		private IModelMetadataProvider _modelMetadataProvider;

		/// <summary>
		/// Constructor.
		/// </summary>
		public QuestionModelBinder(
			IDictionary<Type, IModelBinder> derivedModelBinders, 
			IModelMetadataProvider modelMetadataProvider)
		{
			_derivedModelBinders = derivedModelBinders;
			_modelMetadataProvider = modelMetadataProvider;
		}

		/// <summary>
		/// Binds the model.
		/// </summary>
		public async Task BindModelAsync(ModelBindingContext bindingContext)
		{
			var typeName = (string)bindingContext.ValueProvider
				.GetValue("QuestionType")
				.ConvertTo(typeof(string));

			var derivedModelBinderKvp = _derivedModelBinders.First(kvp => kvp.Key.Name == typeName);
			var derivedModelType = derivedModelBinderKvp.Key;
			var derivedModelBinder = derivedModelBinderKvp.Value;

			ModelBindingResult result;
			using (bindingContext.EnterNestedScope(
				_modelMetadataProvider.GetMetadataForType(derivedModelType),
				bindingContext.FieldName,
				bindingContext.ModelName,
				model: null
				))
			{
				await derivedModelBinder.BindModelAsync(bindingContext);
				result = bindingContext.Result;
			}

			bindingContext.Result = result;
		}
	}

	/// <summary>
	/// A model provider for the QuestionModelBinder.
	/// </summary>
	public class QuestionModelBinderProvider : IModelBinderProvider
	{
		/// <summary>
		/// The types of questions.
		/// </summary>
		private IList<Type> _questionTypes = new List<Type>();

		/// <summary>
		/// Constructor.
		/// </summary>
		public QuestionModelBinderProvider()
		{
		}

		/// <summary>
		/// Returns the model binder for a question.
		/// </summary>
		public IModelBinder GetBinder(ModelBinderProviderContext context)
		{
			if (context.Metadata.ModelType != typeof(Question))
				return null;

			var questionTypes = typeof(Question).GetTypeInfo()
				.Assembly
				.GetTypes()
				.Where(type => typeof(Question).IsAssignableFrom(type))
				.Where(type => !type.GetTypeInfo().IsAbstract)
				.ToList();

			var derivedModelBinders = questionTypes.ToDictionary
			(
				t => t,
				t => context.CreateBinder
				(
					context.MetadataProvider.GetMetadataForType(t)
				)
			);

			return new QuestionModelBinder(derivedModelBinders, context.MetadataProvider);
		}
	}
}
