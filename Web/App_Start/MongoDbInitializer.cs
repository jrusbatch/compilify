using Compilify.Data.Mongo;
using WebActivator;

[assembly: PreApplicationStartMethod(typeof(MongoDbInitializer), "Initialize")]
