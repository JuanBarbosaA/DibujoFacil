import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import Menu from "../../Components/Menu";

export default function TutorialsPage() {
  const [tutorials, setTutorials] = useState([]);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedCategory, setSelectedCategory] = useState("");
  const [selectedDifficulty, setSelectedDifficulty] = useState("");

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [tutorialsRes, categoriesRes] = await Promise.all([
          fetch(
            `http://localhost:5054/api/Tutorials?search=${searchTerm}&categoryId=${selectedCategory}&difficulty=${selectedDifficulty}`
          ),
          fetch("http://localhost:5054/api/Tutorials/categories"),
        ]);

        if (!tutorialsRes.ok || !categoriesRes.ok)
          throw new Error("Error fetching data");

        const tutorialsData = await tutorialsRes.json();
        const categoriesData = await categoriesRes.json();

        setTutorials(tutorialsData);
        setCategories(categoriesData);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, [searchTerm, selectedCategory, selectedDifficulty]);

  const renderFinalResult = (content) => {
    return (
      <div key={content.id} className="space-y-4">
        <div className="px-2 text-center">
          <h3 className="font-bold text-gray-800 text-lg mb-2">
            Resultado Final
          </h3>
          
        </div>
        <div className="aspect-square bg-gray-100 rounded-xl overflow-hidden shadow-lg transform transition hover:scale-105">
          <img
            src={`data:${content.type};base64,${content.contentBase64}`}
            alt="Resultado final del tutorial"
            className="w-full h-full object-cover"
          />
        </div>
      </div>
    );
  };

  
  if (loading)
    return (
      <div className="text-center py-8">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500 mx-auto"></div>
        <p className="mt-4 text-gray-600">Cargando tutoriales...</p>
      </div>
    );

  if (error)
    return (
      <div className="text-center py-8">
        <div className="text-red-500 text-xl mb-4">⚠️ Error</div>
        <p className="text-gray-600">{error}</p>
        <button
          onClick={() => window.location.reload()}
          className="mt-4 px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors"
        >
          Reintentar
        </button>
      </div>
    );

    return (
        <div className="min-h-screen bg-gray-50">
          <section className="bg-white shadow-md flex justify-between items-center py-4 px-4 md:px-20">
            <div>
              <img 
                src="Dibujo-Facil-logo.png" 
                className="w-28 md:w-36 transition-all duration-300 hover:scale-105" 
                alt="Logo Dibujo Fácil"
              />
            </div>
            <Menu />
    
            <Link
              to={"/profile"}
              className="bg-gradient-to-r from-blue-500 to-indigo-500 px-6 py-2.5 rounded-full text-white font-semibold 
                         hover:from-blue-600 hover:to-indigo-600 transition-all duration-300 shadow-sm
                         flex items-center gap-2"
            >
              <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-6-3a2 2 0 11-4 0 2 2 0 014 0zm-2 4a5 5 0 00-4.546 2.916A5.986 5.986 0 0010 16a5.986 5.986 0 004.546-2.084A5 5 0 0010 11z" clipRule="evenodd"/>
              </svg>
              Perfil
            </Link>
          </section>
    
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
            <h1 className="text-4xl font-bold text-gray-900 mb-8 text-center md:text-left">
              Explora Nuestros Tutoriales
              <span className="block mt-2 text-2xl text-blue-600 font-medium">Domina el arte del dibujo</span>
            </h1>
            
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-12">
              <input
                type="text"
                placeholder="Buscar tutoriales..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 
                          transition-shadow duration-200 placeholder-gray-400"
              />
              <select
                value={selectedCategory}
                onChange={(e) => setSelectedCategory(e.target.value)}
                className="px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 
                          bg-white text-gray-700 cursor-pointer"
              >
                <option value="">Todas las categorías</option>
                {categories.map(category => (
                  <option key={category.id} value={category.id}>#{category.name}</option>
                ))}
              </select>
              <select
                value={selectedDifficulty}
                onChange={(e) => setSelectedDifficulty(e.target.value)}
                className="px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 
                          bg-white text-gray-700 cursor-pointer"
              >
                <option value="">Todas las dificultades</option>
                <option value="beginner">Principiante</option>
                <option value="intermediate">Intermedio</option>
                <option value="advanced">Avanzado</option>
              </select>
            </div>
    
            <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-8">
          {tutorials.map(tutorial => {
            const sortedContents = [...tutorial.contents].sort((a, b) => a.order - b.order);
            const finalResult = sortedContents[sortedContents.length - 1];

            return (
              <article 
                key={tutorial.id} 
                className="bg-white rounded-xl shadow-lg hover:shadow-xl transition-all duration-300 transform hover:-translate-y-1 group h-full flex flex-col"
              >
                <Link to={`/tutorial/${tutorial.id}`} className="block hover:no-underline flex flex-col flex-grow">
                  <header className="p-6 bg-gradient-to-r from-blue-50 to-indigo-50 border-b">
                    <h2 className="text-2xl font-bold text-gray-800 group-hover:text-blue-600 transition-colors">
                      {tutorial.title}
                    </h2>
                    <div className="flex items-center mt-4">
                      <img 
                        src={tutorial.author.avatarUrl || '/default-avatar.png'}
                        alt={`Avatar de ${tutorial.author.name}`}
                        className="w-10 h-10 rounded-full mr-3 object-cover border-2 border-white shadow-sm"
                      />
                      <div>
                        <p className="font-medium text-gray-700">{tutorial.author.name}</p>
                        <p className="text-sm text-gray-500">
                          {new Date(tutorial.publicationDate).toLocaleDateString('es-ES', {
                            year: 'numeric',
                            month: 'long',
                            day: 'numeric'
                          })}
                        </p>
                      </div>
                    </div>
                  </header>

                  <div className="p-6 space-y-4  flex-grow">
                    <div className="flex items-center justify-between">
                      <span className={`px-3 py-1 rounded-full text-sm font-semibold tracking-tight ${
                        tutorial.difficulty === 'beginner' ? 'bg-green-500/10 text-green-700' :
                        tutorial.difficulty === 'intermediate' ? 'bg-yellow-500/10 text-yellow-700' :
                        'bg-red-500/10 text-red-700'
                      }`}>
                        {tutorial.difficulty}
                      </span>
                      <div className="flex items-center">
                        <span className="text-yellow-400 text-lg">★</span>
                        <span className="ml-2 font-medium text-gray-700">
                          {tutorial.averageRating.toFixed(1)}
                          <span className="text-sm text-gray-400 ml-1">({tutorial.ratings.length})</span>
                        </span>
                      </div>
                    </div>

                    <div className="flex flex-wrap gap-2">
                      {tutorial.categories.map(category => (
                        <span 
                          key={category.id} 
                          className="px-2.5 py-0.5 bg-gray-100 text-gray-600 text-xs font-medium rounded-full"
                        >
                          #{category.name}
                        </span>
                      ))}
                    </div>
                  </div>

                  <div className="p-6 border-t border-b flex-grow">
                    {finalResult && renderFinalResult(finalResult)}
                  </div>

                  <footer className="p-4 bg-gray-50  mt-auto">
                    <div className="flex items-center justify-between text-sm text-gray-500">
                      <span className="flex items-center">
                        <svg className="w-4 h-4 mr-1.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/>
                        </svg>
                        {tutorial.estimatedDuration} min
                      </span>
                      <span className="flex items-center">
                        <svg className="w-4 h-4 mr-1.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"/>
                        </svg>
                        {tutorial.contents.length} pasos
                      </span>
                    </div>
                  </footer>
                </Link>
              </article>
            );
          })}
        </div>
          </div>
        </div>
      );
}
