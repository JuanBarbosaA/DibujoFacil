import React, { useEffect, useState } from 'react'

export default function DashboardPage() {
    const [tutorials, setTutorials] = useState([])
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState(null)

    useEffect(() => {
        const fetchTutorials = async () => {
            try {
                const response = await fetch('http://localhost:5054/api/Tutorials')
                if (!response.ok) {
                    const errorData = await response.json()
                    throw new Error(errorData.message || 'Error al cargar tutoriales')
                }
                const data = await response.json()
                setTutorials(data)
            } catch (err) {
                setError(err.message)
                console.error('Error detallado:', err)
            } finally {
                setLoading(false)
            }
        }
        fetchTutorials()
    }, [])

    const renderMediaContent = (content) => {
        const mimeType = content.type.startsWith('image/') 
            ? content.type 
            : `image/${content.type}`

        return (
            <div key={content.id} className="space-y-2">
                {/* Descripción y título */}
                <div className="px-2">
                    {content.title && (
                        <h3 className="font-semibold text-gray-800 text-sm mb-1">
                            {content.title}
                        </h3>
                    )}
                    {content.description && (
                        <p className="text-gray-600 text-xs italic">
                            {content.description}
                        </p>
                    )}
                </div>
                
                {/* Imagen */}
                <div className="aspect-square bg-gray-200 rounded-lg overflow-hidden shadow-sm">
                    <img 
                        src={`data:${mimeType};base64,${content.contentBase64}`}
                        alt={content.description || 'Paso del tutorial'}
                        className="w-full h-full object-cover"
                        loading="lazy"
                        decoding="async"
                    />
                </div>
            </div>
        )
    }

    if (loading) return (
        <div className="text-center py-8">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500 mx-auto"></div>
            <p className="mt-4 text-gray-600">Cargando tutoriales...</p>
        </div>
    )

    if (error) return (
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
    )

    return (
        <div className="container mx-auto px-4 py-8">
            <h1 className="text-3xl font-bold text-gray-800 mb-8">Tutoriales de Dibujo</h1>
            
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {tutorials.map(tutorial => (
                    <article key={tutorial.id} className="bg-white rounded-lg shadow-md overflow-hidden hover:shadow-lg transition-shadow duration-300">
                        <header className="p-4 border-b">
                            <h2 className="text-xl font-semibold text-gray-800">{tutorial.title}</h2>
                            <div className="flex items-center mt-2">
                                <img 
                                    src={tutorial.author.avatarUrl || '/default-avatar.png'}
                                    alt={`Avatar de ${tutorial.author.name}`}
                                    className="w-8 h-8 rounded-full mr-2 object-cover"
                                />
                                <span className="text-sm text-gray-600">{tutorial.author.name}</span>
                            </div>
                        </header>

                        <div className="p-4 space-y-2">
                            <div className="flex items-center justify-between">
                                <span className={`px-2 py-1 text-sm rounded-full ${
                                    tutorial.difficulty === 'beginner' ? 'bg-green-100 text-green-800' :
                                    tutorial.difficulty === 'intermediate' ? 'bg-yellow-100 text-yellow-800' :
                                    'bg-red-100 text-red-800'
                                }`}>
                                    {tutorial.difficulty}
                                </span>
                                <div className="flex items-center">
                                    <span className="text-yellow-500">★</span>
                                    <span className="ml-1 text-gray-600">
                                        {tutorial.averageRating.toFixed(1)}
                                    </span>
                                </div>
                            </div>

                            <div className="text-sm text-gray-500">
                                ⌛ {tutorial.estimatedDuration} minutos
                            </div>

                            <div className="flex flex-wrap gap-2">
                                {tutorial.categories.map(category => (
                                    <span 
                                        key={category.id}
                                        className="px-2 py-1 bg-gray-100 text-gray-600 text-sm rounded-full"
                                    >
                                        #{category.name}
                                    </span>
                                ))}
                            </div>
                        </div>

                        {/* Sección de contenidos modificada */}
                        <div className="grid grid-cols-1 gap-4 p-4 bg-gray-50">
                            {tutorial.contents
                                .sort((a, b) => a.order - b.order)
                                .map(renderMediaContent)}
                        </div>

                        <footer className="p-4 border-t">
                            <div className="text-sm text-gray-500">
                                📅 Publicado el {new Date(tutorial.publicationDate).toLocaleDateString('es-ES', {
                                    year: 'numeric',
                                    month: 'long',
                                    day: 'numeric'
                                })}
                            </div>
                        </footer>
                    </article>
                ))}
            </div>
        </div>
    )
}