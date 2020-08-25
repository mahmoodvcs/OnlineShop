using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MahtaKala.Entities.EntityConfig
{
    public class ProductQuantityConfiguration : IEntityTypeConfiguration<ProductQuantity>
    {
        public void Configure(EntityTypeBuilder<ProductQuantity> builder)
        {
            builder.Property(e => e.CharacteristicValues).HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<IList<CharacteristicValue>>(v, (JsonSerializerOptions)null));
        }
    }
}
