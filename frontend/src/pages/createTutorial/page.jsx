import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

export default function CreateTutorialPage() {
    const navigate = useNavigate();
    const [categories, setCategories] = useState([]);
    const [formData, setFormData] = useState({
        title: '',
        description: '',
        difficulty: 'beginner',
        estimatedDuration: 30,
        categoryIds: [],
        contents: []
    });
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');

    useEffect(() => {
        const fetchCategories = async () => {
            try {
                const response = await fetch('http://localhost:5054/api/Tutorials/categories');
                if (!response.ok) throw new Error('Error cargando categorías');
                const data = await response.json();
                setCategories(data);
            } catch (err) {
                setError(err.message);
            }
        };
        fetchCategories();
    }, []);

    const handleFileUpload = (e) => {
        const files = Array.from(e.target.files);
        const newContents = files.map((file, index) => ({
            file,
            type: file.type.startsWith('image/') ? 'image' : 'video',
            order: formData.contents.length + index + 1,
            title: `Paso ${formData.contents.length + index + 1}`,
            description: ''
        }));
        
        setFormData(prev => ({
            ...prev,
            contents: [...prev.contents, ...newContents].sort((a, b) => a.order - b.order)
        }));
    };

    const removeContent = (index) => {
        const newContents = formData.contents.filter((_, i) => i !== index);
        setFormData(prev => ({
            ...prev,
            contents: newContents.map((c, i) => ({ ...c, order: i + 1 }))
        }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setLoading(true);
        setError('');

        try {
            if (formData.contents.length === 0) {
                throw new Error('Debes agregar al menos un paso al tutorial');
            }

            if (formData.categoryIds.length === 0) {
                throw new Error('Selecciona al menos una categoría');
            }

            const formPayload = new FormData();
            
            formPayload.append('Title', formData.title);
            formPayload.append('Description', formData.description);
            formPayload.append('Difficulty', formData.difficulty);
            formPayload.append('EstimatedDuration', formData.estimatedDuration.toString());
            formData.categoryIds.forEach(id => {
                formPayload.append('CategoryIds', id.toString()); 
            });
            
            formData.contents.forEach((content, index) => {
                formPayload.append(`Contents[${index}].File`, content.file);
                formPayload.append(`Contents[${index}].Order`, content.order.toString());
                formPayload.append(`Contents[${index}].Title`, content.title);
                formPayload.append(`Contents[${index}].Description`, content.description);
            });

            const token = localStorage.getItem('token');
            if (!token) throw new Error('Debes iniciar sesión');

            const response = await fetch('http://localhost:5054/api/Tutorials/create', {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`
                },
                body: formPayload
            });
            console.log('Token:', localStorage.getItem('token')); 

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.message || 'Error al crear tutorial');
            }

            navigate('/tutorials');
        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="max-w-4xl mx-auto p-6 bg-white rounded-lg shadow-md">
            <h1 className="text-3xl font-bold text-gray-800 mb-8">Crear Nuevo Tutorial</h1>
            
            <form onSubmit={handleSubmit} className="space-y-6">
                {/* Campo Título */}
                <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                        Título del tutorial *
                    </label>
                    <input
                        type="text"
                        required
                        className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                        value={formData.title}
                        onChange={e => setFormData({...formData, title: e.target.value})}
                    />
                </div>

                {/* Campo Descripción */}
                <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                        Descripción detallada *
                    </label>
                    <textarea
                        required
                        rows="4"
                        className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                        value={formData.description}
                        onChange={e => setFormData({...formData, description: e.target.value})}
                    />
                </div>

                {/* Selectores */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    {/* Dificultad */}
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                            Dificultad *
                        </label>
                        <select
                            className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500"
                            value={formData.difficulty}
                            onChange={e => setFormData({...formData, difficulty: e.target.value})}
                        >
                            <option value="beginner">Principiante</option>
                            <option value="intermediate">Intermedio</option>
                            <option value="advanced">Avanzado</option>
                        </select>
                    </div>

                    {/* Duración */}
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                            Duración estimada (minutos) *
                        </label>
                        <input
                            type="number"
                            min="10"
                            required
                            className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500"
                            value={formData.estimatedDuration}
                            onChange={e => setFormData({...formData, estimatedDuration: e.target.value})}
                        />
                    </div>
                </div>

                {/* Categorías */}
                <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                        Categorías *
                    </label>
                    <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
                        {categories.map(category => (
                            <label 
                                key={category.id} 
                                className="flex items-center space-x-2 p-2 border rounded-lg hover:bg-gray-50"
                            >
                                <input
                                    type="checkbox"
                                    className="form-checkbox h-5 w-5 text-blue-600"
                                    value={category.id}
                                    checked={formData.categoryIds.includes(category.id)}
                                    onChange={e => {
                                        const newCategories = e.target.checked
                                            ? [...formData.categoryIds, category.id]
                                            : formData.categoryIds.filter(id => id !== category.id);
                                        setFormData({...formData, categoryIds: newCategories});
                                    }}
                                />
                                <span className="text-gray-700">{category.name}</span>
                            </label>
                        ))}
                    </div>
                </div>

                {/* Contenido del tutorial */}
                <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                        Contenido del tutorial *
                    </label>
                    
                    {/* Subida de archivos */}
                    <div className="mb-4">
                        <label className="block px-4 py-2 border-2 border-dashed rounded-lg cursor-pointer hover:border-blue-500 transition-colors">
                            <span className="text-gray-600">Arrastra archivos o haz clic para seleccionar</span>
                            <input
                                type="file"
                                multiple
                                accept="image/*,video/*"
                                onChange={handleFileUpload}
                                className="hidden"
                            />
                        </label>
                    </div>

                    {/* Previsualización de contenido */}
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        {formData.contents.map((content, index) => (
                            <div key={index} className="border rounded-lg p-4 bg-gray-50">
                                <div className="relative group">
                                    {/* Miniatura */}
                                    <div className="aspect-video bg-gray-200 rounded-lg overflow-hidden mb-2">
                                        {content.type === 'image' ? (
                                            <img
                                                src={URL.createObjectURL(content.file)}
                                                alt="Previsualización"
                                                className="w-full h-full object-cover"
                                            />
                                        ) : (
                                            <video className="w-full h-full object-cover">
                                                <source src={URL.createObjectURL(content.file)} />
                                            </video>
                                        )}
                                    </div>
                                    
                                    {/* Controles */}
                                    <div className="absolute top-2 right-2 opacity-0 group-hover:opacity-100 transition-opacity">
                                        <button
                                            type="button"
                                            onClick={() => removeContent(index)}
                                            className="p-1 bg-red-500 text-white rounded-full hover:bg-red-600"
                                        >
                                            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                                            </svg>
                                        </button>
                                    </div>
                                </div>

                                {/* Campos editables */}
                                <input
                                    type="text"
                                    placeholder="Título del paso"
                                    className="w-full mb-2 px-2 py-1 text-sm border rounded"
                                    value={content.title}
                                    onChange={e => {
                                        const newContents = [...formData.contents];
                                        newContents[index].title = e.target.value;
                                        setFormData({...formData, contents: newContents});
                                    }}
                                />
                                <textarea
                                    placeholder="Descripción del paso"
                                    className="w-full px-2 py-1 text-sm border rounded"
                                    value={content.description}
                                    rows="2"
                                    onChange={e => {
                                        const newContents = [...formData.contents];
                                        newContents[index].description = e.target.value;
                                        setFormData({...formData, contents: newContents});
                                    }}
                                />
                            </div>
                        ))}
                    </div>
                </div>

                {/* Botón de envío y mensajes de error */}
                <div className="pt-6">
                    {error && (
                        <div className="mb-4 p-3 bg-red-100 text-red-700 rounded-lg">
                            {error}
                        </div>
                    )}
                    
                    <button
                        type="submit"
                        disabled={loading}
                        className="w-full bg-blue-600 text-white py-3 px-6 rounded-lg hover:bg-blue-700 transition-colors disabled:bg-gray-400 flex justify-center items-center"
                    >
                        {loading ? (
                            <>
                                <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                                </svg>
                                Publicando...
                            </>
                        ) : (
                            'Publicar Tutorial'
                        )}
                    </button>
                </div>
            </form>
        </div>
    );
}