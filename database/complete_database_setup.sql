-- ============================================================================
-- IHEC Library - Complete Database Setup Script for New Supabase Database
-- Database URL: https://luneenvyunhuvrhpsoig.supabase.co
-- Generated: $(date)
-- ============================================================================

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ============================================================================
-- 1. DROP EXISTING TABLES (if they exist) - For clean setup
-- ============================================================================

-- Drop tables in reverse dependency order
DROP TABLE IF EXISTS activitylogs CASCADE;
DROP TABLE IF EXISTS aichathistory CASCADE;
DROP TABLE IF EXISTS booksofinterest CASCADE;
DROP TABLE IF EXISTS bookcomments CASCADE;
DROP TABLE IF EXISTS booklikes CASCADE;
DROP TABLE IF EXISTS bookratings CASCADE;
DROP TABLE IF EXISTS bookreservations CASCADE;
DROP TABLE IF EXISTS bookborrowings CASCADE;
DROP TABLE IF EXISTS bookstatistics CASCADE;
DROP TABLE IF EXISTS books CASCADE;
DROP TABLE IF EXISTS adminprofiles CASCADE;
DROP TABLE IF EXISTS studentprofiles CASCADE;
DROP TABLE IF EXISTS users CASCADE;

-- ============================================================================
-- 2. CREATE MAIN TABLES
-- ============================================================================

-- Users table (consolidated to include profile information)
CREATE TABLE users (
    user_id UUID PRIMARY KEY DEFAULT auth.uid(),
    email VARCHAR NOT NULL UNIQUE,
    password_hash VARCHAR NOT NULL DEFAULT '',
    first_name VARCHAR NOT NULL,
    last_name VARCHAR NOT NULL,
    phone_number VARCHAR,
    profile_picture_url TEXT,
    level_of_study VARCHAR,
    field_of_study VARCHAR,
    is_student BOOLEAN NOT NULL DEFAULT TRUE,
    is_admin BOOLEAN NOT NULL DEFAULT FALSE,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    books_borrowed INTEGER DEFAULT 0,
    books_reserved INTEGER DEFAULT 0,
    ranking VARCHAR DEFAULT 'Bronze',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    last_login TIMESTAMPTZ
);

-- Student profiles table (additional student-specific data)
CREATE TABLE studentprofiles (
    student_id UUID PRIMARY KEY REFERENCES users(user_id) ON DELETE CASCADE,
    level_of_study VARCHAR,
    field_of_study VARCHAR,
    books_borrowed INTEGER DEFAULT 0,
    books_reserved INTEGER DEFAULT 0,
    ranking VARCHAR DEFAULT 'Bronze',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Admin profiles table
CREATE TABLE adminprofiles (
    admin_id UUID PRIMARY KEY REFERENCES users(user_id) ON DELETE CASCADE,
    job_title VARCHAR NOT NULL,
    is_approved BOOLEAN NOT NULL DEFAULT FALSE,
    approved_by UUID REFERENCES users(user_id),
    approved_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Books table
CREATE TABLE books (
    book_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title VARCHAR NOT NULL,
    author VARCHAR NOT NULL,
    publication_year INTEGER,
    publisher VARCHAR,
    isbn VARCHAR UNIQUE,
    description TEXT,
    cover_image_url TEXT,
    page_count INTEGER,
    language VARCHAR DEFAULT 'French',
    category VARCHAR,
    tags TEXT[],
    availability_status VARCHAR DEFAULT 'Available',
    available_copies INTEGER DEFAULT 1,
    total_copies INTEGER DEFAULT 1,
    likes_count INTEGER DEFAULT 0,
    rating_average DECIMAL(3,2) DEFAULT 0.00,
    rating_count INTEGER DEFAULT 0,
    added_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    added_by UUID REFERENCES users(user_id)
);

-- Book statistics table
CREATE TABLE bookstatistics (
    book_id UUID PRIMARY KEY REFERENCES books(book_id) ON DELETE CASCADE,
    total_borrows INTEGER DEFAULT 0,
    total_reservations INTEGER DEFAULT 0,
    average_rating NUMERIC(3,2) DEFAULT 0,
    total_ratings INTEGER DEFAULT 0,
    total_likes INTEGER DEFAULT 0,
    total_comments INTEGER DEFAULT 0,
    last_borrowed TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Book borrowings table
CREATE TABLE bookborrowings (
    borrowing_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    book_id UUID NOT NULL REFERENCES books(book_id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    borrow_date TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    due_date TIMESTAMPTZ NOT NULL,
    return_date TIMESTAMPTZ,
    is_returned BOOLEAN NOT NULL DEFAULT FALSE,
    reminder_sent BOOLEAN NOT NULL DEFAULT FALSE,
    late_fee DECIMAL(10,2) DEFAULT 0.00,
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Book reservations table
CREATE TABLE bookreservations (
    reservation_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    book_id UUID NOT NULL REFERENCES books(book_id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    reservation_date TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    expiry_date TIMESTAMPTZ,
    status VARCHAR DEFAULT 'pending',
    notification_sent BOOLEAN NOT NULL DEFAULT FALSE,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Book ratings table
CREATE TABLE bookratings (
    rating_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    book_id UUID NOT NULL REFERENCES books(book_id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    rating INTEGER NOT NULL CHECK (rating BETWEEN 1 AND 5),
    review_text TEXT,
    is_approved BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE(book_id, user_id)
);

-- Book likes table
CREATE TABLE booklikes (
    like_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    book_id UUID NOT NULL REFERENCES books(book_id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE(book_id, user_id)
);

-- Book comments table
CREATE TABLE bookcomments (
    comment_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    book_id UUID NOT NULL REFERENCES books(book_id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    comment_text TEXT NOT NULL,
    is_approved BOOLEAN DEFAULT TRUE,
    parent_comment_id UUID REFERENCES bookcomments(comment_id),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Books of interest table (wishlist)
CREATE TABLE booksofinterest (
    interest_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    book_id UUID NOT NULL REFERENCES books(book_id) ON DELETE CASCADE,
    priority INTEGER DEFAULT 1,
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE(user_id, book_id)
);

-- AI chat history table
CREATE TABLE aichathistory (
    chat_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    user_message TEXT NOT NULL,
    ai_response TEXT NOT NULL,
    session_id UUID,
    message_type VARCHAR DEFAULT 'general',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Activity logs table
CREATE TABLE activitylogs (
    log_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(user_id) ON DELETE SET NULL,
    activity_type VARCHAR NOT NULL,
    activity_description TEXT NOT NULL,
    title VARCHAR,
    entity_type VARCHAR,
    entity_id UUID,
    ip_address INET,
    user_agent TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ============================================================================
-- 3. CREATE INDEXES FOR PERFORMANCE
-- ============================================================================

-- Users table indexes
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_is_admin ON users(is_admin);
CREATE INDEX idx_users_is_student ON users(is_student);
CREATE INDEX idx_users_level_field ON users(level_of_study, field_of_study);
CREATE INDEX idx_users_created_at ON users(created_at);

-- Books table indexes
CREATE INDEX idx_books_title ON books USING gin(to_tsvector('english', title));
CREATE INDEX idx_books_author ON books USING gin(to_tsvector('english', author));
CREATE INDEX idx_books_category ON books(category);
CREATE INDEX idx_books_availability ON books(availability_status);
CREATE INDEX idx_books_isbn ON books(isbn);
CREATE INDEX idx_books_rating ON books(rating_average DESC);
CREATE INDEX idx_books_likes ON books(likes_count DESC);
CREATE INDEX idx_books_created_at ON books(added_at DESC);

-- Borrowings table indexes
CREATE INDEX idx_borrowings_user_id ON bookborrowings(user_id);
CREATE INDEX idx_borrowings_book_id ON bookborrowings(book_id);
CREATE INDEX idx_borrowings_due_date ON bookborrowings(due_date);
CREATE INDEX idx_borrowings_is_returned ON bookborrowings(is_returned);
CREATE INDEX idx_borrowings_borrow_date ON bookborrowings(borrow_date DESC);

-- Reservations table indexes
CREATE INDEX idx_reservations_user_id ON bookreservations(user_id);
CREATE INDEX idx_reservations_book_id ON bookreservations(book_id);
CREATE INDEX idx_reservations_status ON bookreservations(status);
CREATE INDEX idx_reservations_active ON bookreservations(is_active);
CREATE INDEX idx_reservations_date ON bookreservations(reservation_date);

-- Ratings table indexes
CREATE INDEX idx_ratings_book_id ON bookratings(book_id);
CREATE INDEX idx_ratings_user_id ON bookratings(user_id);
CREATE INDEX idx_ratings_rating ON bookratings(rating);
CREATE INDEX idx_ratings_created_at ON bookratings(created_at DESC);

-- Likes table indexes
CREATE INDEX idx_likes_book_id ON booklikes(book_id);
CREATE INDEX idx_likes_user_id ON booklikes(user_id);
CREATE INDEX idx_likes_created_at ON booklikes(created_at DESC);

-- Comments table indexes
CREATE INDEX idx_comments_book_id ON bookcomments(book_id);
CREATE INDEX idx_comments_user_id ON bookcomments(user_id);
CREATE INDEX idx_comments_approved ON bookcomments(is_approved);
CREATE INDEX idx_comments_created_at ON bookcomments(created_at DESC);

-- Activity logs indexes
CREATE INDEX idx_activity_user_id ON activitylogs(user_id);
CREATE INDEX idx_activity_type ON activitylogs(activity_type);
CREATE INDEX idx_activity_created_at ON activitylogs(created_at DESC);

-- AI chat history indexes
CREATE INDEX idx_chat_user_id ON aichathistory(user_id);
CREATE INDEX idx_chat_session_id ON aichathistory(session_id);
CREATE INDEX idx_chat_created_at ON aichathistory(created_at DESC);

-- ============================================================================
-- 4. CREATE FUNCTIONS AND TRIGGERS
-- ============================================================================

-- Function to update updated_at column
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Apply update triggers to all tables with updated_at
CREATE TRIGGER update_users_updated_at
    BEFORE UPDATE ON users
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_books_updated_at
    BEFORE UPDATE ON books
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_borrowings_updated_at
    BEFORE UPDATE ON bookborrowings
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_reservations_updated_at
    BEFORE UPDATE ON bookreservations
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_ratings_updated_at
    BEFORE UPDATE ON bookratings
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_comments_updated_at
    BEFORE UPDATE ON bookcomments
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Function to update book statistics when ratings change
CREATE OR REPLACE FUNCTION update_book_rating_stats()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        UPDATE books SET 
            rating_average = (
                SELECT ROUND(AVG(rating)::numeric, 2) 
                FROM bookratings 
                WHERE book_id = NEW.book_id
            ),
            rating_count = (
                SELECT COUNT(*) 
                FROM bookratings 
                WHERE book_id = NEW.book_id
            )
        WHERE book_id = NEW.book_id;
        RETURN NEW;
    ELSIF TG_OP = 'DELETE' THEN
        UPDATE books SET 
            rating_average = (
                SELECT COALESCE(ROUND(AVG(rating)::numeric, 2), 0) 
                FROM bookratings 
                WHERE book_id = OLD.book_id
            ),
            rating_count = (
                SELECT COUNT(*) 
                FROM bookratings 
                WHERE book_id = OLD.book_id
            )
        WHERE book_id = OLD.book_id;
        RETURN OLD;
    ELSIF TG_OP = 'UPDATE' THEN
        UPDATE books SET 
            rating_average = (
                SELECT ROUND(AVG(rating)::numeric, 2) 
                FROM bookratings 
                WHERE book_id = NEW.book_id
            ),
            rating_count = (
                SELECT COUNT(*) 
                FROM bookratings 
                WHERE book_id = NEW.book_id
            )
        WHERE book_id = NEW.book_id;
        RETURN NEW;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- Trigger for rating statistics
CREATE TRIGGER update_book_rating_stats_trigger
    AFTER INSERT OR UPDATE OR DELETE ON bookratings
    FOR EACH ROW EXECUTE FUNCTION update_book_rating_stats();

-- Function to update likes count
CREATE OR REPLACE FUNCTION update_book_likes_count()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        UPDATE books SET likes_count = likes_count + 1 WHERE book_id = NEW.book_id;
        RETURN NEW;
    ELSIF TG_OP = 'DELETE' THEN
        UPDATE books SET likes_count = likes_count - 1 WHERE book_id = OLD.book_id;
        RETURN OLD;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- Trigger for likes count
CREATE TRIGGER update_book_likes_count_trigger
    AFTER INSERT OR DELETE ON booklikes
    FOR EACH ROW EXECUTE FUNCTION update_book_likes_count();

-- Function to update available copies on borrow/return
CREATE OR REPLACE FUNCTION update_available_copies()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' AND NEW.is_returned = FALSE THEN
        -- New borrowing
        UPDATE books SET available_copies = available_copies - 1 
        WHERE book_id = NEW.book_id AND available_copies > 0;
        RETURN NEW;
    ELSIF TG_OP = 'UPDATE' AND OLD.is_returned = FALSE AND NEW.is_returned = TRUE THEN
        -- Book returned
        UPDATE books SET available_copies = available_copies + 1 
        WHERE book_id = NEW.book_id;
        RETURN NEW;
    ELSIF TG_OP = 'DELETE' AND OLD.is_returned = FALSE THEN
        -- Borrowing record deleted (restore copy)
        UPDATE books SET available_copies = available_copies + 1 
        WHERE book_id = OLD.book_id;
        RETURN OLD;
    END IF;
    RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;

-- Trigger for available copies
CREATE TRIGGER update_available_copies_trigger
    AFTER INSERT OR UPDATE OR DELETE ON bookborrowings
    FOR EACH ROW EXECUTE FUNCTION update_available_copies();

-- Function to calculate user ranking
CREATE OR REPLACE FUNCTION calculate_user_ranking(p_user_id UUID)
RETURNS VARCHAR AS $$
DECLARE
    borrow_count INTEGER;
    user_rank VARCHAR;
BEGIN
    SELECT COUNT(*) INTO borrow_count 
    FROM bookborrowings 
    WHERE user_id = p_user_id AND is_returned = TRUE;
    
    IF borrow_count < 2 THEN
        user_rank := 'Bronze';
    ELSIF borrow_count BETWEEN 2 AND 4 THEN
        user_rank := 'Silver';
    ELSIF borrow_count BETWEEN 5 AND 10 THEN
        user_rank := 'Gold';
    ELSE
        user_rank := 'Master';
    END IF;
    
    RETURN user_rank;
END;
$$ LANGUAGE plpgsql;

-- Function to get book recommendations
CREATE OR REPLACE FUNCTION get_book_recommendations(p_user_id UUID, p_limit INTEGER DEFAULT 10)
RETURNS TABLE (
    book_id UUID,
    title VARCHAR,
    author VARCHAR,
    category VARCHAR,
    cover_image_url TEXT,
    available_copies INTEGER,
    rating_average DECIMAL
) AS $$
BEGIN
    RETURN QUERY
    WITH user_preferences AS (
        SELECT b.category, COUNT(*) as category_count
        FROM booklikes bl
        JOIN books b ON bl.book_id = b.book_id
        WHERE bl.user_id = p_user_id
        GROUP BY b.category
        UNION
        SELECT b.category, COUNT(*) * 2 as category_count
        FROM bookratings br
        JOIN books b ON br.book_id = b.book_id
        WHERE br.user_id = p_user_id AND br.rating >= 4
        GROUP BY b.category
    ),
    user_field AS (
        SELECT field_of_study FROM users WHERE user_id = p_user_id
    )
    SELECT b.book_id, b.title, b.author, b.category, b.cover_image_url, 
           b.available_copies, b.rating_average
    FROM books b
    LEFT JOIN bookborrowings bb ON b.book_id = bb.book_id AND bb.user_id = p_user_id
    WHERE 
        bb.borrowing_id IS NULL
        AND b.available_copies > 0
        AND (
            b.category IN (SELECT category FROM user_preferences)
            OR b.category ILIKE '%' || (SELECT field_of_study FROM user_field) || '%'
            OR b.rating_average >= 4.0
        )
    ORDER BY 
        (SELECT COALESCE(SUM(category_count), 0) FROM user_preferences up WHERE up.category = b.category) DESC,
        b.rating_average DESC,
        b.added_at DESC
    LIMIT p_limit;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- 5. ROW LEVEL SECURITY POLICIES
-- ============================================================================

-- Enable RLS on all tables
ALTER TABLE users ENABLE ROW LEVEL SECURITY;
ALTER TABLE studentprofiles ENABLE ROW LEVEL SECURITY;
ALTER TABLE adminprofiles ENABLE ROW LEVEL SECURITY;
ALTER TABLE books ENABLE ROW LEVEL SECURITY;
ALTER TABLE bookstatistics ENABLE ROW LEVEL SECURITY;
ALTER TABLE bookborrowings ENABLE ROW LEVEL SECURITY;
ALTER TABLE bookreservations ENABLE ROW LEVEL SECURITY;
ALTER TABLE bookratings ENABLE ROW LEVEL SECURITY;
ALTER TABLE booklikes ENABLE ROW LEVEL SECURITY;
ALTER TABLE bookcomments ENABLE ROW LEVEL SECURITY;
ALTER TABLE booksofinterest ENABLE ROW LEVEL SECURITY;
ALTER TABLE aichathistory ENABLE ROW LEVEL SECURITY;
ALTER TABLE activitylogs ENABLE ROW LEVEL SECURITY;

-- Users policies
CREATE POLICY "Users can view own profile" ON users
    FOR SELECT USING (auth.uid() = user_id);

CREATE POLICY "Users can update own profile" ON users
    FOR UPDATE USING (auth.uid() = user_id);

CREATE POLICY "Admins can view all users" ON users
    FOR SELECT USING (
        EXISTS (SELECT 1 FROM users WHERE user_id = auth.uid() AND is_admin = TRUE)
    );

-- Books policies (all authenticated users can read)
CREATE POLICY "Anyone can view books" ON books
    FOR SELECT USING (true);

CREATE POLICY "Admins can manage books" ON books
    FOR ALL USING (
        EXISTS (SELECT 1 FROM users WHERE user_id = auth.uid() AND is_admin = TRUE)
    );

-- Borrowings policies
CREATE POLICY "Users can view own borrowings" ON bookborrowings
    FOR SELECT USING (auth.uid() = user_id);

CREATE POLICY "Users can create borrowings" ON bookborrowings
    FOR INSERT WITH CHECK (auth.uid() = user_id);

CREATE POLICY "Admins can view all borrowings" ON bookborrowings
    FOR SELECT USING (
        EXISTS (SELECT 1 FROM users WHERE user_id = auth.uid() AND is_admin = TRUE)
    );

-- Reservations policies
CREATE POLICY "Users can view own reservations" ON bookreservations
    FOR SELECT USING (auth.uid() = user_id);

CREATE POLICY "Users can create reservations" ON bookreservations
    FOR INSERT WITH CHECK (auth.uid() = user_id);

-- Ratings policies
CREATE POLICY "Anyone can view ratings" ON bookratings
    FOR SELECT USING (true);

CREATE POLICY "Users can manage own ratings" ON bookratings
    FOR ALL USING (auth.uid() = user_id);

-- Likes policies
CREATE POLICY "Users can manage own likes" ON booklikes
    FOR ALL USING (auth.uid() = user_id);

-- Comments policies
CREATE POLICY "Anyone can view approved comments" ON bookcomments
    FOR SELECT USING (is_approved = true);

CREATE POLICY "Users can manage own comments" ON bookcomments
    FOR ALL USING (auth.uid() = user_id);

-- Chat history policies
CREATE POLICY "Users can view own chat history" ON aichathistory
    FOR ALL USING (auth.uid() = user_id);

-- Activity logs policies
CREATE POLICY "Admins can view activity logs" ON activitylogs
    FOR SELECT USING (
        EXISTS (SELECT 1 FROM users WHERE user_id = auth.uid() AND is_admin = TRUE)
    );

-- ============================================================================
-- 6. INITIAL DATA SETUP
-- ============================================================================

-- Insert sample admin user (you should update this with real data)
INSERT INTO users (
    user_id, 
    email, 
    password_hash, 
    first_name, 
    last_name, 
    is_admin, 
    is_student
) VALUES (
    gen_random_uuid(),
    'admin@ihec.ucar.tn',
    crypt('admin123', gen_salt('bf')),
    'System',
    'Administrator',
    true,
    false
) ON CONFLICT (email) DO NOTHING;

-- ============================================================================
-- 7. VIEWS FOR COMMON QUERIES
-- ============================================================================

-- View for book details with statistics
CREATE OR REPLACE VIEW book_details AS
SELECT 
    b.*,
    COALESCE(bs.total_borrows, 0) as total_borrows,
    COALESCE(bs.total_reservations, 0) as total_reservations,
    COALESCE(bs.total_comments, 0) as total_comments
FROM books b
LEFT JOIN bookstatistics bs ON b.book_id = bs.book_id;

-- View for user statistics
CREATE OR REPLACE VIEW user_stats AS
SELECT 
    u.user_id,
    u.first_name,
    u.last_name,
    u.email,
    COUNT(DISTINCT bb.borrowing_id) as total_borrowings,
    COUNT(DISTINCT br.reservation_id) as total_reservations,
    COUNT(DISTINCT bl.like_id) as total_likes,
    COUNT(DISTINCT rt.rating_id) as total_ratings,
    COALESCE(AVG(rt.rating), 0) as average_rating_given
FROM users u
LEFT JOIN bookborrowings bb ON u.user_id = bb.user_id
LEFT JOIN bookreservations br ON u.user_id = br.user_id
LEFT JOIN booklikes bl ON u.user_id = bl.user_id
LEFT JOIN bookratings rt ON u.user_id = rt.user_id
GROUP BY u.user_id, u.first_name, u.last_name, u.email;

-- ============================================================================
-- SETUP COMPLETE
-- ============================================================================

-- Display completion message
DO $$
BEGIN
    RAISE NOTICE '============================================================================';
    RAISE NOTICE 'IHEC Library database setup completed successfully!';
    RAISE NOTICE 'Database URL: https://luneenvyunhuvrhpsoig.supabase.co';
    RAISE NOTICE '============================================================================';
    RAISE NOTICE 'Tables created: %', (
        SELECT COUNT(*) FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_type = 'BASE TABLE'
    );
    RAISE NOTICE 'Indexes created: %', (
        SELECT COUNT(*) FROM pg_indexes WHERE schemaname = 'public'
    );
    RAISE NOTICE 'Functions created: %', (
        SELECT COUNT(*) FROM information_schema.routines 
        WHERE routine_schema = 'public' AND routine_type = 'FUNCTION'
    );
    RAISE NOTICE '============================================================================';
END $$;
