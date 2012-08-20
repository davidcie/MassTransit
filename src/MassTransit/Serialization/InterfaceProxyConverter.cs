// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Serialization
{
	using System;
	using Magnum.Reflection;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	public class InterfaceProxyConverter :
		JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			// try to deserialize first, perhaps type information is
			// hidden within the message and Json.NET can do the job
			object direct = serializer.Deserialize(reader);
			if (direct != null && objectType.IsInstanceOfType(direct))
			{
				return direct;
			}

			// direct deserialization failed, we need a proxy type after all
			Type proxyType = InterfaceImplementationBuilder.GetProxyFor(objectType);
			object proxy = FastActivator.Create(proxyType);
			using (var tokens = new JTokenReader((JObject)direct))
			{
				serializer.Populate(tokens, proxy);
			}
			return proxy;
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType.IsInterface && objectType.IsAllowedMessageType();
		}
	}
}