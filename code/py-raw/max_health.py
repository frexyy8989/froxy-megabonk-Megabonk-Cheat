import pymem
import pymem.process

# 1. Oyuna bağlan (Örnek: megabonk.exe ise onu yaz)
try:
    pm = pymem.Pymem("Megabonk.exe") # Burayı oyunun .exe adıyla değiştir!
    print("Oyuna başarıyla bağlanıldı.")
except Exception as e:
    print(f"Hata: Oyun bulunamadı veya yönetici olarak çalıştırılmadı. {e}")
    exit()

# 2. GameAssembly.dll modülünün başlangıç adresini bul
module = pymem.process.module_from_name(pm.process_handle, "GameAssembly.dll").lpBaseOfDll

# 3. Pointer Okuma Fonksiyonu (Dinamik adresi hesaplar)
def get_pointer_address(base, offsets):
    # Oyun muhtemelen 64-bit (1A9... ile başladığı için), read_longlong kullanıyoruz
    addr = pm.read_longlong(base)
    for offset in offsets[:-1]:
        addr = pm.read_longlong(addr + offset)
    return addr + offsets[-1]

# --- SENİN GÖRÜNTÜNDEKİ DEĞERLER ---
# Base Static Address: GameAssembly.dll + 02F840B8
base_static = module + 0x02F60EA8

# Offsetler: En alttan en üste doğru sıralı liste
# Görüntündeki kutucukları aşağıdan yukarıya doğru yazdık
my_offsets = [0x20, 0xB8, 0x30, 0x78, 0x48, 0x20, 0x14]

try:
    # Dinamik adresi bul
    dynamic_address = get_pointer_address(base_static, my_offsets)
    print(f"Hesaplanan Dinamik Adres: {hex(dynamic_address)}")

    # Şu anki değeri oku (Resimde 999 yazıyor, doğrulamak için basalım)
    current_value = pm.read_int(dynamic_address)
    print(f"Şuanki maksimum can: {current_value}")
    i = input("Ayarlanacak maksimum can: ")
    pm.write_int(dynamic_address,int(i))
    print(f"Yeni maksimum can: {i}")

    # EĞER DEĞERİ DEĞİŞTİRMEK İSTERSEN:
    # pm.write_int(dynamic_address, 1337)
    # print("Değer başarıyla değiştirildi!")

except Exception as e:
    print(f"Adres hesaplanırken hata oluştu (Oyun kapalı veya harita yüklenmemiş olabilir): {e}")