import React, { useState, useRef, useEffect } from 'react';
import { Link } from 'react-router-dom';

export default function CommentSection({ tutorialId }) {
    const [commentText, setCommentText] = useState('');
    const [comments, setComments] = useState([]);
    const [currentPage, setCurrentPage] = useState(1);
    const [totalComments, setTotalComments] = useState(0);
    const [hasMore, setHasMore] = useState(false);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);
    const textareaRef = useRef(null);
    const isLoggedIn = !!localStorage.getItem('token');
    const user = JSON.parse(localStorage.getItem('user'));

    const fetchComments = async (page) => {
        setLoading(true);
        try {
            const response = await fetch(`http://localhost:5054/api/Tutorials/${tutorialId}/comments?page=${page}&pageSize=4`);
            if (!response.ok) throw new Error('Error al cargar comentarios');
            const data = await response.json();
            setComments(prev => page === 1 ? data.comments : [...prev, ...data.comments]);
            setTotalComments(data.totalComments);
            setHasMore(data.hasMore);
            setError('');
        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchComments(currentPage);
    }, [currentPage, tutorialId]);

    const handleSubmit = async () => {
        setIsSubmitting(true);
        setError('');
        try {
            const token = localStorage.getItem('token');
            if (!token || !isLoggedIn) throw new Error('Debes iniciar sesión');
            if (!commentText.trim()) throw new Error('Comentario vacío');

            const response = await fetch(`http://localhost:5054/api/Tutorials/${tutorialId}/comment`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({ text: commentText })
            });

            if (!response.ok) throw new Error('Error al enviar');
            
            const newComment = await response.json();
            setComments(prev => [newComment, ...prev]);
            setTotalComments(prev => prev + 1);
            setCommentText('');
            textareaRef.current.focus();
        } catch (err) {
            setError(err.message);
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <section className="border-t pt-8">
            <div className="flex items-center justify-between mb-6">
                <h2 className="text-2xl font-bold text-gray-900">
                    💬 Comentarios ({totalComments})
                </h2>
            </div>

            {isLoggedIn && (
                <div className="mb-8 bg-white p-6 rounded-xl shadow-sm border border-gray-200">
                    <div className="flex gap-4">
                        <img
                            src={user?.avatarUrl || '/default-avatar.png'}
                            alt={user?.name}
                            className="w-12 h-12 rounded-full object-cover"
                        />
                        <div className="flex-1">
                            <textarea
                                ref={textareaRef}
                                value={commentText}
                                onChange={(e) => setCommentText(e.target.value)}
                                placeholder="Escribe tu comentario..."
                                className="w-full p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                rows="3"
                                disabled={isSubmitting}
                            />
                            
                            <div className="mt-4 flex justify-between items-center">
                                <button
                                    onClick={handleSubmit}
                                    disabled={isSubmitting || !commentText.trim()}
                                    className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50"
                                >
                                    {isSubmitting ? 'Enviando...' : 'Publicar'}
                                </button>
                                
                                <span className={`text-sm ${commentText.length > 1000 ? 'text-red-600' : 'text-gray-500'}`}>
                                    {commentText.length}/1000
                                </span>
                            </div>

                            {error && (
                                <p className="mt-2 text-red-600 flex items-center">
                                    <svg className="w-5 h-5 mr-2" fill="currentColor" viewBox="0 0 20 20">
                                        <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd"/>
                                    </svg>
                                    {error}
                                </p>
                            )}
                        </div>
                    </div>
                </div>
            )}

            {loading && currentPage === 1 && (
                <div className="text-center py-4">
                    <div className="animate-spin rounded-full h-8 w-8 border-4 border-blue-500 border-t-transparent"></div>
                </div>
            )}

            {comments.map(comment => (
                <div key={comment.id} className="mb-6 bg-white p-5 rounded-xl shadow-sm border border-gray-200">
                    <div className="flex items-center gap-4 mb-4">
                        <img
                            src={comment.user.avatarUrl || '/default-avatar.png'}
                            alt={comment.user.name}
                            className="w-12 h-12 rounded-full object-cover"
                        />
                        <div>
                            <p className="font-medium text-gray-900">{comment.user.name}</p>
                            <p className="text-sm text-gray-500">
                                {new Date(comment.date).toLocaleDateString('es-ES', { day: 'numeric', month: 'short', year: 'numeric' })}
                                {comment.edited && <span className="ml-2 text-gray-400 text-xs">(editado)</span>}
                            </p>
                        </div>
                    </div>
                    <p className="text-gray-700 whitespace-pre-wrap">{comment.text}</p>
                </div>
            ))}

            {hasMore && (
                <div className="mt-6 text-center">
                    <button 
                        onClick={() => setCurrentPage(prev => prev + 1)}
                        disabled={loading}
                        className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50"
                    >
                        {loading ? 'Cargando...' : 'Ver más comentarios'}
                    </button>
                </div>
            )}

            {!loading && comments.length === 0 && (
                <div className="text-center py-8 text-gray-500 bg-white rounded-xl border border-gray-200">
                    Aún no hay comentarios. ¡Sé el primero en opinar!
                </div>
            )}

            {!isLoggedIn && (
                <div className="text-center py-6 bg-blue-50 rounded-lg">
                    <p className="text-gray-600">
                        <Link to="/login" className="text-blue-600 hover:underline font-medium">Inicia sesión</Link> para comentar
                    </p>
                </div>
            )}
        </section>
    );
};