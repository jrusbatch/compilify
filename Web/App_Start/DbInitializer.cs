using Compilify.DataAccess.MongoDB;
using WebActivator;

[assembly: PreApplicationStartMethod(typeof(MongoDbInitializer), "Initialize")]
