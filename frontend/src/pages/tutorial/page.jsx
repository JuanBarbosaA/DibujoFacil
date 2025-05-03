import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import CommentSection from '../../Components/CommentSection';
import DownloadPdfButton from '../../Components/DownloadPdfButton';

export default function TutorialPage() {
    const { id } = useParams();
    const [tutorial, setTutorial] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [selectedRating, setSelectedRating] = useState(0);
    const [ratingError, setRatingError] = useState('');
    const [hasRated, setHasRated] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);

    const fetchTutorialData = async () => {
        try {
            const response = await fetch(`http://localhost:5054/api/Tutorials/${id}`);
            if (!response.ok) throw new Error('Error al cargar el tutorial');
            const data = await response.json();
            return data;
        } catch (error) {
            throw new Error('Error al obtener los datos del tutorial', error);
        }
    };

    const checkUserRating = (ratings) => {
        const user = JSON.parse(localStorage.getItem('user'));
        return user ? ratings.some(r => r.user.id === user.id) : false;
    };

    const handleRatingSubmit = async () => {
        setIsSubmitting(true);
        setRatingError('');
        
        try {
            const token = localStorage.getItem('token');
            if (!token) throw new Error('Debes iniciar sesión para calificar');

            const response = await fetch(`http://localhost:5054/api/Tutorials/${id}/rate`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({ score: selectedRating })
            });

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.message || 'Error al enviar calificación');
            }

            const updatedData = await fetchTutorialData();
            setTutorial(updatedData);
            setHasRated(true);
        } catch (err) {
            setRatingError(err.message);
        } finally {
            setIsSubmitting(false);
        }
    };

    
    useEffect(() => {
        const loadData = async () => {
            try {
                const data = await fetchTutorialData();
                setTutorial(data);
                setHasRated(checkUserRating(data.ratings));
                setError('');
            } catch (err) {
                setError(err.message);
            } finally {
                setLoading(false);
            }
        };
        loadData();
    }, [id]);

    const RatingSection = () => {
        const [hoverRating, setHoverRating] = useState(0);
        const isLoggedIn = !!localStorage.getItem('token');

        return (
            <div className="bg-white p-6 rounded-lg shadow-md my-8 border border-gray-200">
                <h3 className="text-xl font-bold mb-4">
                    {hasRated ? 'Tu Calificación' : 'Calificar Tutorial'}
                </h3>
                
                <div className="flex items-center gap-4">
                    <div className="flex gap-3">
                        {[1, 2, 3, 4, 5].map((star) => (
                            <button
                                key={star}
                                className={`relative text-4xl transition-all duration-200 ${
                                    isLoggedIn && !hasRated ? 
                                    'cursor-pointer hover:scale-110' : 'cursor-default'
                                } ${
                                    (hoverRating >= star || selectedRating >= star) ? 
                                    'text-yellow-500 drop-shadow-star' : 'text-gray-200'
                                }`}
                                onMouseEnter={() => !hasRated && isLoggedIn && setHoverRating(star)}
                                onMouseLeave={() => !hasRated && isLoggedIn && setHoverRating(0)}
                                onClick={() => !hasRated && isLoggedIn && setSelectedRating(star)}
                                disabled={hasRated || !isLoggedIn}
                            >
                                <div className="absolute inset-0 opacity-0 group-hover:opacity-40 transition-opacity">
                                    <div className="absolute inset-0 bg-yellow-200 blur-[12px]"></div>
                                </div>
                                <div className="relative">★</div>
                            </button>
                        ))}
                    </div>

                    {!isLoggedIn && (
                        <p className="text-gray-600 text-sm">
                            <Link to="/login" className="text-blue-600 hover:underline">
                                Inicia sesión
                            </Link> para calificar
                        </p>
                    )}

                    {selectedRating > 0 && isLoggedIn && !hasRated && (
                        <button
                            onClick={handleRatingSubmit}
                            disabled={isSubmitting}
                            className="ml-4 px-4 py-2 bg-blue-600 text-white rounded-lg 
                                    hover:bg-blue-700 transition-colors disabled:opacity-50"
                        >
                            {isSubmitting ? 'Enviando...' : 'Enviar'}
                        </button>
                    )}
                </div>

                {hasRated && (
                    <p className="mt-4 text-green-600 flex items-center">
                        <svg className="w-5 h-5 mr-2" fill="currentColor" viewBox="0 0 20 20">
                            <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z"/>
                        </svg>
                        ¡Gracias por tu calificación!
                    </p>
                )}

                {ratingError && (
                    <p className="mt-4 text-red-600 flex items-center">
                        <svg className="w-5 h-5 mr-2" fill="currentColor" viewBox="0 0 20 20">
                            <path
                                fillRule="evenodd"
                                d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
                                clipRule="evenodd"
                            />
                        </svg>
                        {ratingError}
                    </p>
                )}
            </div>
        );
    };

    

   


    if (loading) {
        return (
            <div className="min-h-screen flex items-center justify-center">
                <div className="text-center">
                    <div className="animate-spin rounded-full h-12 w-12 border-4 border-blue-500 border-t-transparent"></div>
                    <p className="mt-4 text-gray-600">Cargando tutorial...</p>
                </div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="min-h-screen flex items-center justify-center">
                <div className="text-center max-w-md">
                    <div className="text-red-500 text-5xl mb-4">⚠️</div>
                    <h2 className="text-xl font-semibold mb-4">Error al cargar el tutorial</h2>
                    <p className="text-gray-600 mb-6">{error}</p>
                    <Link
                        to="/"
                        className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
                    >
                        Volver al inicio
                    </Link>
                </div>
            </div>
        );
    }

    return (
        <div className="container mx-auto px-4 py-8 max-w-4xl">
            <Link to="/" className="inline-flex items-center text-blue-600 hover:text-blue-700 mb-6">
                <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M10 19l-7-7m0 0l7-7m-7 7h18"/>
                </svg>
                Volver a todos los tutoriales
            </Link>

            <article>
                <header className="mb-8">
                    <h1 className="text-4xl font-bold text-gray-900 mb-4">{tutorial.title}</h1>
                    <div className="flex items-center gap-4">
                        <img
                            src={tutorial.author.avatarUrl || '/default-avatar.png'}
                            alt={tutorial.author.name}
                            className="w-14 h-14 rounded-full object-cover border-2 border-white shadow"
                        />
                        <div>
                            <p className="font-semibold text-gray-900">{tutorial.author.name}</p>
                            <p className="text-sm text-gray-500">
                                Publicado el {new Date(tutorial.publicationDate).toLocaleDateString('es-ES', {
                                    day: 'numeric',
                                    month: 'long',
                                    year: 'numeric'
                                })}
                            </p>
                        </div>
                    </div>
                </header>

                <div className="bg-gray-50 p-6 rounded-xl mb-8 shadow-sm">
                    <div className="flex flex-wrap gap-4 items-center mb-4">
                        <span className={`px-4 py-2 rounded-full font-medium ${
                            tutorial.difficulty === 'beginner' ? 'bg-green-100 text-green-800' :
                            tutorial.difficulty === 'intermediate' ? 'bg-yellow-100 text-yellow-800' :
                            'bg-red-100 text-red-800'
                        }`}>
                            {tutorial.difficulty}
                        </span>
                        <div className="flex items-center gap-2">
                            <span className="text-yellow-500">★</span>
                            <span className="font-medium text-gray-700">
                                {tutorial.averageRating.toFixed(1)}/5.0
                            </span>
                            <span className="text-gray-500 text-sm">
                                ({tutorial.ratings.length} calificaciones)
                            </span>
                        </div>
                    </div>
                    <div className="flex flex-wrap gap-2">
                        {tutorial.categories.map(category => (
                            <span
                                key={category.id}
                                className="px-3 py-1 bg-blue-100 text-blue-800 text-sm rounded-full"
                            >
                                #{category.name}
                            </span>
                        ))}
                    </div>
                </div>
                
                <DownloadPdfButton />
                
                <RatingSection />

                <section className="prose max-w-none mb-12">
                    <p className="text-lg text-gray-600 leading-relaxed">{tutorial.description}</p>

                    {tutorial.contents.sort((a, b) => a.order - b.order).map((content, index) => (
                        <div key={content.id} className="my-10">
                            <div className="mb-6 border-l-4 border-blue-600 pl-4">
                                <h3 className="text-2xl font-semibold text-gray-900">
                                    Paso {index + 1}: {content.title}
                                </h3>
                                {content.description && (
                                    <p className="mt-2 text-gray-600">{content.description}</p>
                                )}
                            </div>
                            <img
                                src={`data:${content.type};base64,${content.contentBase64}`}
                                alt={content.description || `Paso ${index + 1}`}
                                className="rounded-xl shadow-lg w-full h-auto"
                            />
                        </div>
                    ))}
                </section>
   

                <CommentSection tutorialId={id} />
                
             
            </article>
        </div>
    );
}